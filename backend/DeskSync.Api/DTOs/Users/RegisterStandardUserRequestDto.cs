
using System.ComponentModel.DataAnnotations;

namespace DeskSync.Api.DTOs.Users;

public record RegisterStandardUserRequestDto (
    [MaxLength(100, ErrorMessage="First name must be at most 100 characters long")]
    string? FirstName,

    [MaxLength(100, ErrorMessage="Last name must be at most 100 characters long")]
    string? LastName,

    [Required(ErrorMessage="Username is required")]
    [MaxLength(100, ErrorMessage="Username must be at most 100 characters long")]
    string Username,

    [Required(ErrorMessage="Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    string Email,

    [MaxLength(64, ErrorMessage="Password must be at most 64 characters long")]
    [MinLength(16, ErrorMessage ="Password must be at least 16 characters long")]
    string? Password
);
