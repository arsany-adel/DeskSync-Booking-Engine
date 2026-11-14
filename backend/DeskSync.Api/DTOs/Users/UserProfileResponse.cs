namespace DeskSync.Api.DTOs.Users;

public record UserProfileResponse(
    Guid Id,
    string Email,
    string Username,
    string? FirstName,
    string? LastName,
    string Role,
    bool EmailConfirmed,
    bool TwoFactorEnabled,
    IReadOnlyList<string> LinkedProviders
);