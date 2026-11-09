using DeskSync.Api.Configuration;
using DeskSync.Api.Constants;
using DeskSync.Api.DTOs.Users;
using DeskSync.Api.Entities;
using DeskSync.Api.Extensions.Mappers;
using DeskSync.Api.Repositories.Interfaces;
using DeskSync.Api.Services.Interfaces;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NodaTime;
using OtpNet;

namespace DeskSync.Api.Services;

public class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenService tokenService,
    IEmailService emailService,
    IClock clock,
    IOptions<FrontendSettingsOptions> frontendOptions,
    IPasswordHasher<User> passwordHasher) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IEmailService _emailService = emailService;
    private readonly IClock _clock = clock;
    private readonly string _frontendBaseUrl = frontendOptions.Value.BaseUrl;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
    private const string AuthenticatorIssuer = "DeskSync";

    public async Task<ErrorOr<AuthResponse>> RegisterStandardUserAsync(RegisterStandardUserRequestDto dto)
    {
        if (!await _userRepository.IsEmailUnique(dto.Email))
            return DomainErrors.Auth.EmailAlreadyInUse;

        if (!await _userRepository.IsUsernameUnique(dto.Username))
            return DomainErrors.Auth.UsernameAlreadyInUse;

        User user = dto.ToEntity(passwordHash: null);

        string hashedPassword = _passwordHasher.HashPassword(user, dto.Password);
        user.UpdatePassword(hashedPassword);
        string confirmationToken = _tokenService.GenerateSecureToken();
        user.SetEmailConfirmationToken(confirmationToken);

        await _userRepository.AddUserAsync(user);
        await _userRepository.SaveChangesAsync();

        // Send confirmation email
        string confirmLink = $"{_frontendBaseUrl}/confirm-email?token={confirmationToken}";

        await _emailService.SendEmailAsync(
            toEmail: user.Email,
            subject: "Welcome to DeskSync — Please Confirm Your Email",
            htmlBody: $@"
                <h2>Welcome to DeskSync, {user.Username}!</h2>
                <p>Please confirm your email address by clicking the link below:</p>
                <p><a href=""{confirmLink}"">Confirm My Email</a></p>
                <p>If you didn't create this account, you can safely ignore this email.</p>",
            plainTextBody: $"Welcome to DeskSync, {user.Username}!\n\nPlease confirm your email:\n{confirmLink}");

        var refreshToken = _tokenService.GenerateRefreshToken(user);
        await _refreshTokenRepository.AddAsync(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthResponse(
            _tokenService.GenerateAccessToken(user),
            refreshToken.Token,
            user.Id, user.Email, user.Username, user.Role.ToString());
    }

    public async Task<ErrorOr<AuthResponse>> LoginStandardUserAsync(LoginUserRequestDto dto)
    {
        User? user = await _userRepository.GetUserByEmailAsync(dto.Email);

        if (user == null)
            return DomainErrors.Auth.InvalidCredentials;

        // PRD Edge Case A: Social-only users cannot use password login
        if (user.PasswordHash == null)
            return DomainErrors.Auth.AccountHasNoPassword;

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, dto.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
            return DomainErrors.Auth.InvalidCredentials;

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.UpdatePassword(_passwordHasher.HashPassword(user, dto.Password));
            await _userRepository.SaveChangesAsync();
        }

        if (user.Role == UserRole.Admin && !user.TwoFactorEnabled)
        {
            return DomainErrors.Auth.AdminRequiresTwoFactor;
        }

        if (user.TwoFactorEnabled)
        {
            return new AuthResponse(
                AccessToken: string.Empty, 
                RefreshToken: string.Empty,
                UserId: user.Id,
                Email: user.Email,
                Username: user.Username,
                Role: "RequiresTwoFactor");
        }

        var refreshToken = _tokenService.GenerateRefreshToken(user);
        await _refreshTokenRepository.AddAsync(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthResponse(
            _tokenService.GenerateAccessToken(user),
            refreshToken.Token,
            user.Id, user.Email, user.Username, user.Role.ToString());
    }

    public async Task<ErrorOr<AuthResponse>> RefreshTokenAsync(string refreshToken)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (token == null)
            return DomainErrors.Auth.InvalidToken;

        // Reuse detection: a revoked token being presented means either
        // (a) the client replayed an old token, or (b) it was stolen.
        // Treat as compromise and revoke every active token for this user.
        if (token.RevokedAt != null)
        {
            await _refreshTokenRepository.RevokeAllForUserAsync(
                token.UserId, _clock.GetCurrentInstant());
            return DomainErrors.Auth.InvalidToken;
        }

        if (token.ExpiresAt < _clock.GetCurrentInstant())
            return DomainErrors.Auth.TokenExpired;

        var user = token.User;

        // Rotate: revoke the old one, issue a new one
        token.Revoke();
        var newRefreshToken = _tokenService.GenerateRefreshToken(user);
        await _refreshTokenRepository.AddAsync(newRefreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthResponse(
            _tokenService.GenerateAccessToken(user),
            newRefreshToken.Token,
            user.Id, user.Email, user.Username, user.Role.ToString());
    }

    public async Task<ErrorOr<Success>> LogoutAsync(string refreshToken)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (token != null && token.RevokedAt == null)
        {
            token.Revoke();
            await _refreshTokenRepository.SaveChangesAsync();
        }

        return Result.Success;
    }

    public async Task<ErrorOr<Success>> ForgotPasswordAsync(ForgotPasswordRequest dto)
    {
        User? user = await _userRepository.GetUserByEmailAsync(dto.Email);

        // Security: Always return success even if user not found (prevents email enumeration)
        if (user == null)
            return Result.Success;

        string resetToken = _tokenService.GenerateSecureToken();
        var expiry = _clock.GetCurrentInstant().Plus(Duration.FromHours(1));

        user.SetPasswordResetToken(resetToken, expiry);
        await _userRepository.SaveChangesAsync();

        string resetLink = $"{_frontendBaseUrl}/reset-password?token={resetToken}";

        await _emailService.SendEmailAsync(
            toEmail: user.Email,
            subject: "DeskSync — Password Reset Request",
            htmlBody: $@"
                <h2>Reset Your Password</h2>
                <p>You requested a password reset. Click the link below to choose a new password:</p>
                <p><a href=""{resetLink}"">Reset My Password</a></p>
                <p>This link will expire in 1 hour. If you didn't request this, you can ignore this email.</p>",
            plainTextBody: $"Reset your password here:\n{resetLink}\n\nThis link expires in 1 hour.");

        return Result.Success;
    }

    public async Task<ErrorOr<Success>> ResetPasswordAsync(ResetPasswordRequest dto)
    {
        User? user = await _userRepository.GetUserByPasswordResetTokenAsync(dto.Token);

        if (user == null || user.PasswordResetTokenExpiresAt == null)
            return DomainErrors.Auth.InvalidToken;

        if (user.PasswordResetTokenExpiresAt < _clock.GetCurrentInstant())
            return DomainErrors.Auth.TokenExpired;

        string hashedPassword = _passwordHasher.HashPassword(user, dto.NewPassword);
        user.UpdatePassword(hashedPassword);
        user.ClearPasswordResetToken();

        await _userRepository.SaveChangesAsync();

        return Result.Success;
    }

    public async Task<ErrorOr<Success>> ConfirmEmailAsync(ConfirmEmailRequest dto)
    {
        User? user = await _userRepository.GetUserByEmailConfirmationTokenAsync(dto.Token);

        if (user == null)
            return DomainErrors.Auth.InvalidToken;

        user.ConfirmEmail();
        await _userRepository.SaveChangesAsync();

        return Result.Success;
    }

    public async Task<ErrorOr<EnableTwoFactorResponse>> EnableTwoFactorAsync(Guid userId)
    {
        User? user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null) return DomainErrors.Auth.InvalidCredentials;

        if (user.TwoFactorEnabled) return DomainErrors.Auth.TwoFactorAlreadyEnabled;

        // 20-byte secret (160 bits, standard for TOTP)
        var secretBytes = KeyGeneration.GenerateRandomKey(20);
        string base32Secret = Base32Encoding.ToString(secretBytes);

        user.SetPendingTwoFactorSecret(base32Secret);
        await _userRepository.SaveChangesAsync();

        // Build otpauth:// URI for the frontend to render as a QR code
        string otpauthUri =
            $"otpauth://totp/{AuthenticatorIssuer}:{Uri.EscapeDataString(user.Email)}" +
            $"?secret={base32Secret}&issuer={AuthenticatorIssuer}&algorithm=SHA1&digits=6&period=30";

        return new EnableTwoFactorResponse(base32Secret, otpauthUri);
    }

    public async Task<ErrorOr<Success>> VerifyAndActivateTwoFactorAsync(Guid userId, VerifyTwoFactorRequest dto)
    {
        User? user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null || user.TwoFactorSecret == null)
            return DomainErrors.Auth.TwoFactorNotActivated;

        if (user.TwoFactorEnabled) return DomainErrors.Auth.TwoFactorAlreadyEnabled;

        if (!VerifyTotpCode(user.TwoFactorSecret, dto.Code))
            return DomainErrors.Auth.InvalidTwoFactorCode;

        user.ActivateTwoFactor();
        await _userRepository.SaveChangesAsync();
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> DisableTwoFactorAsync(Guid userId, DisableTwoFactorRequest dto)
    {
        User? user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null || user.PasswordHash == null)
            return DomainErrors.Auth.InvalidCredentials;

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
            return DomainErrors.Auth.InvalidCredentials;

        if (!user.TwoFactorEnabled) return DomainErrors.Auth.TwoFactorNotEnabled;

        user.DisableTwoFactor();
        await _userRepository.SaveChangesAsync();
        return Result.Success;
    }

    public async Task<ErrorOr<AuthResponse>> VerifyTwoFactorLoginAsync(VerifyTwoFactorLoginRequest dto)
    {
        User? user = await _userRepository.GetUserByEmailAsync(dto.Email);
        if (user == null || user.TwoFactorSecret == null || !user.TwoFactorEnabled)
            return DomainErrors.Auth.TwoFactorNotEnabled;

        if (!VerifyTotpCode(user.TwoFactorSecret, dto.Code))
            return DomainErrors.Auth.InvalidTwoFactorCode;

        var refreshToken = _tokenService.GenerateRefreshToken(user);
        await _refreshTokenRepository.AddAsync(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthResponse(
            _tokenService.GenerateAccessToken(user),
            refreshToken.Token,
            user.Id, user.Email, user.Username, user.Role.ToString());
    }

    private static bool VerifyTotpCode(string base32Secret, string code)
    {
        var secretBytes = Base32Encoding.ToBytes(base32Secret);
        var totp = new Totp(secretBytes);
        // VerifyTotp checks current window ± a few seconds of drift
        return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
    }

    public async Task<ErrorOr<UserProfileResponse>> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            return DomainErrors.Auth.InvalidCredentials;

        var providers = await _userRepository.GetLinkedProvidersAsync(userId);

        return new UserProfileResponse(
            user.Id,
            user.Email,
            user.Username,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.EmailConfirmed,
            user.TwoFactorEnabled,
            providers.Select(p => p.ToString()).ToList());
    }
}