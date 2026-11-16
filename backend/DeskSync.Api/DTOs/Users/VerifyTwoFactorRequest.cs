using System.ComponentModel.DataAnnotations;

namespace DeskSync.Api.DTOs.Users;

public record VerifyTwoFactorRequest(
    [Required(ErrorMessage = "Authenticator code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
    string Code
);