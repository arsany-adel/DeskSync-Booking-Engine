namespace DeskSync.Api.DTOs.Users;

public record ExternalUserInfo(
    string ProviderKey,
    string Email,
    bool EmailVerified,
    string? FirstName,
    string? LastName,
    string? Username
);