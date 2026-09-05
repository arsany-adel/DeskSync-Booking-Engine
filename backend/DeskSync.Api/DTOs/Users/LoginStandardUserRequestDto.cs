using System.ComponentModel.DataAnnotations;

namespace DeskSync.Api.DTOs.Users;
public record LoginUserRequestDto(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    string Email,

    [Required(ErrorMessage = "Password is required")]
    string Password
);

