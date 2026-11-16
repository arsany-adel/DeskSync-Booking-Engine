namespace DeskSync.Api.DTOs.Users;

public record LogoutAllRequest(
    string? CurrentRefreshToken = null
);