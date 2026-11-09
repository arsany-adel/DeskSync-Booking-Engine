using DeskSync.Api.Constants;
using DeskSync.Api.DTOs.Users;
using DeskSync.Api.Entities;
using DeskSync.Api.Repositories.Interfaces;
using DeskSync.Api.Services.Interfaces;
using ErrorOr;
using NodaTime;

namespace DeskSync.Api.Services;

public class SocialAuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenService tokenService,
    IClock clock) : ISocialAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IClock _clock = clock;

    public async Task<ErrorOr<AuthResponse>> HandleExternalLoginAsync(
        ProviderType provider,
        ExternalUserInfo externalUser)
    {
        if (!externalUser.EmailVerified)
            return DomainErrors.Auth.OAuthEmailNotVerified;

        // 1. Check if this social identity is already linked
        var existingLogin = await _userRepository.GetUserLoginAsync(provider, externalUser.ProviderKey);

        if (existingLogin != null)
        {
            return await IssueTokensAsync(existingLogin.User);
        }

        // 2. Not linked. Check if a user with this email exists
        var userByEmail = await _userRepository.GetUserByEmailAsync(externalUser.Email);

        if (userByEmail != null)
        {
            // PRD Edge Case C: Auto-link ONLY if the provider says the email is verified (we already checked above)
            var newLogin = new UserLogin(provider, externalUser.ProviderKey, userByEmail.Id);
            await _userRepository.AddUserLoginAsync(newLogin);
            await _userRepository.SaveChangesAsync();
            return await IssueTokensAsync(userByEmail);
        }

        // 3. Brand new user. Create without a password.
        var username = !string.IsNullOrWhiteSpace(externalUser.Username)
            ? externalUser.Username
            : externalUser.Email.Split('@')[0];

        if (!await _userRepository.IsUsernameUnique(username))
            username = $"{username}_{Guid.NewGuid().ToString("N")[..6]}";

        var newUser = new User(
            id: Guid.NewGuid(),
            username: username,
            firstName: externalUser.FirstName,
            lastName: externalUser.LastName,
            email: externalUser.Email,
            passwordHash: null,
            role: UserRole.Standard,
            emailNotificationEnabled: true);

        newUser.ConfirmEmail();

        await _userRepository.AddUserAsync(newUser);

        var userLogin = new UserLogin(provider, externalUser.ProviderKey, newUser.Id);
        await _userRepository.AddUserLoginAsync(userLogin);

        await _userRepository.SaveChangesAsync();

        return await IssueTokensAsync(newUser);
    }

    private async Task<ErrorOr<AuthResponse>> IssueTokensAsync(User user)
    {
        var refreshToken = _tokenService.GenerateRefreshToken(user);
        await _refreshTokenRepository.AddAsync(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthResponse(
            _tokenService.GenerateAccessToken(user),
            refreshToken.Token,
            user.Id,
            user.Email,
            user.Username,
            user.Role.ToString());
    }
}