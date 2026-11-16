using System.ComponentModel.DataAnnotations;

namespace DeskSync.Api.DTOs.Users;

public record RefreshTokenRequest(
    [Required(ErrorMessage = "Refresh token is required")]
    string RefreshToken
);