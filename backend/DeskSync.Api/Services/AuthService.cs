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

namespace DeskSync.Api.Services;

public class AuthService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IEmailService emailService,
    IClock clock,
    IOptions<FrontendSettingsOptions> frontendOptions,
    IPasswordHasher<User> passwordHasher) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IEmailService _emailService = emailService;
    private readonly IClock _clock = clock;
    private readonly string _frontendBaseUrl = frontendOptions.Value.BaseUrl;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;

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

        return new AuthResponse(
            _tokenService.GenerateAccessToken(user),
            _tokenService.GenerateRefreshToken(),
            user.Id,
            user.Email,
            user.Username,
            user.Role.ToString()
        );
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

        return new AuthResponse(
            _tokenService.GenerateAccessToken(user),
            _tokenService.GenerateRefreshToken(),
            user.Id,
            user.Email,
            user.Username,
            user.Role.ToString()
        );
    }

    public Task<ErrorOr<AuthResponse>> RefreshTokenAsync(string refreshToken)
    {
        return Task.FromResult<ErrorOr<AuthResponse>>(DomainErrors.Auth.RefreshNotImplemented);
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
}