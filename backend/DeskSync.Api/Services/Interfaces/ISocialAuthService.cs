using DeskSync.Api.DTOs.Users;
using DeskSync.Api.Entities;
using ErrorOr;

namespace DeskSync.Api.Services.Interfaces;

public interface ISocialAuthService
{
    Task<ErrorOr<AuthResponse>> HandleExternalLoginAsync(
        ProviderType provider,
        ExternalUserInfo externalUser);
    Task<ErrorOr<Success>> LinkExternalLoginAsync(
        Guid userId, ProviderType provider, ExternalUserInfo externalUser);

    Task<ErrorOr<Success>> UnlinkAsync(Guid userId, ProviderType provider);
}