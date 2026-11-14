namespace DeskSync.Api.DTOs.Users;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    string Email,
    string Username,
    string Role
);