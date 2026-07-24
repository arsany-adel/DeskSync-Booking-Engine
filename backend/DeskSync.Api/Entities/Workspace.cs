namespace DeskSync.Api.Entities;

public class Workspace
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = null;
    public string? LogoUrl { get; set; } = null;
    public string Address { get; set; } = string.Empty;
    public string GeographicalLocation { get; set; } = string.Empty;
    public string? GoogleMapsLocation {get; set; } = null;
}
