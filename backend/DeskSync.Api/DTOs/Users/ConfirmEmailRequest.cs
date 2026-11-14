using System.ComponentModel.DataAnnotations;

namespace DeskSync.Api.DTOs.Users;

public record ConfirmEmailRequest(
    [Required(ErrorMessage = "Token is required")]
    string Token
);