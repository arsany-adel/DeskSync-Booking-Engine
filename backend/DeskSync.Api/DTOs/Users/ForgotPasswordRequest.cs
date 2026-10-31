using System.ComponentModel.DataAnnotations;

namespace DeskSync.Api.DTOs.Users;

public record ForgotPasswordRequest(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    string Email
);