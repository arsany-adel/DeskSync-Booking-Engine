using DeskSync.Api.Entities;

namespace DeskSync.Api.Services.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string GenerateSecureToken();
}