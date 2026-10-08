using System.ComponentModel.DataAnnotations;

namespace DeskSync.Api.Configuration;

public class SmtpSettingsOptions
{
    public const string SectionName = "Smtp";

    [Required(ErrorMessage = "SMTP host is required.")]
    public string Host { get; init; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "SMTP port must be between 1 and 65535.")]
    public int Port { get; init; } = 587;

    /// <summary>
    /// true  → STARTTLS (typically port 587)
    /// false → implicit TLS / SslOnConnect (typically port 465)
    /// </summary>
    public bool UseStartTls { get; init; } = true;

    /// <summary>Optional. Leave empty for unauthenticated relays (MailHog, local dev).</summary>
    public string? Username { get; init; }

    public string? Password { get; init; }

    [Required(ErrorMessage = "SMTP from address is required.")]
    [EmailAddress(ErrorMessage = "SMTP from address must be a valid email.")]
    public string FromAddress { get; init; } = string.Empty;
}