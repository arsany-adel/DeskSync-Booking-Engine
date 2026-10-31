using System.ComponentModel.DataAnnotations;

namespace DeskSync.Api.Configuration;

public class JwtSettingsOptions
{
    public const string SectionName = "Jwt";

    [Required(ErrorMessage = "JWT SecretKey is required.")]
    [MinLength(32, ErrorMessage = "JWT SecretKey must be at least 32 characters long.")]
    public string SecretKey { get; init; } = string.Empty;

    [Required(ErrorMessage = "JWT Issuer is required.")]
    public string Issuer { get; init; } = "DeskSync";

    [Required(ErrorMessage = "JWT Audience is required.")]
    public string Audience { get; init; } = "DeskSyncFrontend";

    [Range(1, 1440)]
    public int AccessTokenMinutes { get; init; } = 15;

    [Range(1, 365)]
    public int RefreshTokenDays { get; init; } = 7;
}