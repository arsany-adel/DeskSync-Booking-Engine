using System.ComponentModel.DataAnnotations;

namespace DeskSync.Api.DTOs.Users;

public record DisableTwoFactorRequest(
    [Required(ErrorMessage = "Password is required to disable 2FA")]
    string Password
);