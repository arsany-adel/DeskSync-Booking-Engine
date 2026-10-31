using System.ComponentModel.DataAnnotations;

namespace DeskSync.Api.DTOs.Users;

public record ResetPasswordRequest(
    [Required(ErrorMessage = "Token is required")]
    string Token,

    [Required(ErrorMessage = "New password is required")]
    [MaxLength(64, ErrorMessage = "Password must be at most 64 characters long")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    string NewPassword
);