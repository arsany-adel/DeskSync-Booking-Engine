using System.ComponentModel.DataAnnotations;

namespace DeskSync.Api.Configuration;

public class SmtpSettingsOptions
{
    public const string SectionName = "Smtp";

    [Required(ErrorMessage = "SMTP host is required.")]
    public string Host { get; init; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "SMTP port must be between 1 and 65535.")]
    public int Port { get; init; } = 587;

    public bool UseStartTls { get; init; } = true;

    public string? Username { get; init; }

    public string? Password { get; init; }

    [Required(ErrorMessage = "SMTP from address is required.")]
    [EmailAddress(ErrorMessage = "SMTP from address must be a valid email.")]
    public string FromAddress { get; init; } = string.Empty;

    [Required(ErrorMessage = "AppBaseUrl is required.")]
    [Url(ErrorMessage = "AppBaseUrl must be a valid URL.")]
    public string AppBaseUrl { get; init; } = "https://app.desksync.com";
}