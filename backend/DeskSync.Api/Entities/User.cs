namespace DeskSync.Api.Entities;

public enum UserRole
{
    Standard,
    Admin
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public UserRole Role { get; set; }
    public string? FirstName { get; set; } = null;
    public string? LastName { get; set; } = null;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; } = null;
    public bool EmailNotificationEnabled { get; set; } = false;
}
