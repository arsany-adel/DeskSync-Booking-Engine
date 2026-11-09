using DeskSync.Api.DTOs.Users;
using DeskSync.Api.Entities;
using ErrorOr;

namespace DeskSync.Api.Services.Interfaces;

public interface ISocialAuthService
{
    Task<ErrorOr<AuthResponse>> HandleExternalLoginAsync(
        ProviderType provider,
        ExternalUserInfo externalUser);
}