using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DeskSync.Api.Configuration;
using DeskSync.Api.Entities;
using DeskSync.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NodaTime;

namespace DeskSync.Api.Services;

public class TokenService(
    IOptions<JwtSettingsOptions> jwtOptions, IClock clock) : ITokenService
{
    private readonly JwtSettingsOptions _jwt = jwtOptions.Value;
    private readonly IClock _clock = clock;

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("Username", user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(User user)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var tokenString = Convert.ToBase64String(randomBytes);

        var now = _clock.GetCurrentInstant();
        var expiry = now.Plus(Duration.FromDays(_jwt.RefreshTokenDays));

        return new RefreshToken(
            id: Guid.NewGuid(),
            userId: user.Id,
            token: tokenString,
            createdAt: now,
            expiresAt: expiry);
    }

    public string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}