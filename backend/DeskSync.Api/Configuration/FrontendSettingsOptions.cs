using System.ComponentModel.DataAnnotations;

namespace DeskSync.Api.Configuration;

public class FrontendSettingsOptions
{
    public const string SectionName = "Frontend";

    [Required(ErrorMessage = "Frontend BaseUrl is required.")]
    [Url(ErrorMessage = "Frontend BaseUrl must be a valid URL.")]
    public string BaseUrl { get; init; } = string.Empty;
}