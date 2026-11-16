using DeskSync.Api.Entities;
using ErrorOr;

namespace DeskSync.Api.Services.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateSecureToken();
    RefreshToken GenerateRefreshToken(User user);

    string GenerateOAuthState(Guid? userId, string provider);
    ErrorOr<Guid?> ValidateOAuthState(string state, string expectedProvider);
}