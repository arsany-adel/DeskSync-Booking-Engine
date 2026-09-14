
using System.Security.Claims;
using DeskSync.Api.DTOs.Users;
using DeskSync.Api.Entities;
using DeskSync.Api.Extensions.Mappers;
using DeskSync.Api.Repositories.Interfaces;
using DeskSync.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace DeskSync.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task RegisterStandardUserAsync(RegisterStandardUserRequestDto dto)
    {
        if (!await _userRepository.IsEmailUnique(dto.Email))
            throw new InvalidOperationException("Email is already in use");

        if (!await _userRepository.IsUsernameUnique(dto.Username))
            throw new InvalidOperationException("Username is already in use");

        User user = dto.ToEntity(passwordHash: null); 

        string hashedPassword = _passwordHasher.HashPassword(user, dto.Password!);

        user.UpdatePassword(hashedPassword);

        await _userRepository.AddUserAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<ClaimsPrincipal> LoginStandardUserAsync(LoginUserRequestDto dto)
    {
        User? user = await _userRepository.GetUserByEmailAsync(dto.Email);

        if (user == null || user.PasswordHash == null)
            throw new UnauthorizedAccessException("Invalid Credentials");
        
        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user, 
            user.PasswordHash, 
            dto.Password
        );

        if (verificationResult == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid Credentials");


        // True if a new hashing algorithm for the built-in library has been introduced
        // It will re-hash the old password based on the new algorithm
        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.UpdatePassword(_passwordHasher.HashPassword(user, dto.Password));
            await _userRepository.SaveChangesAsync();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("Username", user.Username),
        };

        var identity = new ClaimsIdentity(claims, "Bearer");
        return new ClaimsPrincipal(identity);
    } 
}
