namespace DeskSync.Api.Entities;

public class Workspace
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public string Address { get; private set; }
    public string? GoogleMapsLocation {get; private set; }

    public Workspace(
        Guid id,
        string name,
        string? description,
        string? logoUrl,
        string address,
        string? googleMapsLocation
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address is required");

        Id = id;
        Name = name;
        Description = description;
        LogoUrl = logoUrl;
        Address = address;
        GoogleMapsLocation = googleMapsLocation;
    }

#pragma warning disable CS8618
    private Workspace() { }
#pragma warning restore CS8618

    public void UpdateWorkspace(
        string name,
        string? description,
        string? logoUrl,
        string address,
        string? googleMapsLocation
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address is required");

        Name = name;
        Description = description;
        LogoUrl = logoUrl;
        Address = address;
        GoogleMapsLocation = googleMapsLocation;
    }
}