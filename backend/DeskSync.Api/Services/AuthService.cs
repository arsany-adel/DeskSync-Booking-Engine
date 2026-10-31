using DeskSync.Api.Constants;
using DeskSync.Api.DTOs.Users;
using DeskSync.Api.Entities;
using DeskSync.Api.Extensions.Mappers;
using DeskSync.Api.Repositories.Interfaces;
using DeskSync.Api.Services.Interfaces;
using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace DeskSync.Api.Services;

public class AuthService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IPasswordHasher<User> passwordHasher) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ITokenService _tokenService = tokenService;
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

        // Phase 1 baseline: mark email as confirmed for now.
        // Phase 2 will change this to send a confirmation email.
        user.ConfirmEmail();

        await _userRepository.AddUserAsync(user);
        await _userRepository.SaveChangesAsync();

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

        if (user == null || user.PasswordHash == null)
            return DomainErrors.Auth.InvalidCredentials;

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
        // Phase 2 will implement a proper refresh token store.
        // For Phase 1, we return a NotImplemented error so no broken flow ships.
        return Task.FromResult<ErrorOr<AuthResponse>>(DomainErrors.Auth.RefreshNotImplemented);
    }
}