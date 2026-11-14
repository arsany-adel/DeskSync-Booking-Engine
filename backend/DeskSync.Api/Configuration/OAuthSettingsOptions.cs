using System.ComponentModel.DataAnnotations;

namespace DeskSync.Api.Configuration;

public class OAuthSettingsOptions
{
    public const string SectionName = "OAuth";

    [Required]
    public ProviderSettings Google { get; init; } = new();

    [Required]
    public ProviderSettings Microsoft { get; init; } = new();

    public class ProviderSettings
    {
        [Required(ErrorMessage = "ClientId is required.")]
        public string ClientId { get; init; } = string.Empty;

        [Required(ErrorMessage = "ClientSecret is required.")]
        public string ClientSecret { get; init; } = string.Empty;
    }
}