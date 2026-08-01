namespace DeskSync.Api.Entities;

public enum UserRole
{
    Standard,
    Admin
}

public class User
{
    public Guid Id { get; private set; }
    public UserRole Role { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string? Password { get; private set; }
    public bool EmailNotificationEnabled { get; private set; }

    public User(
        Guid id,
        string username,
        string? firstName,
        string? lastName,
        string email,
        string? password,
        UserRole role = UserRole.Standard,
        bool emailNotificationEnabled = false
    )
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required");

        Id = id;
        Role = role;
        FirstName = firstName;
        LastName = lastName;
        Username = username;
        Email = email;
        Password = password;
        EmailNotificationEnabled = emailNotificationEnabled;
    }

#pragma warning disable CS8618
    private User() { }
#pragma warning restore CS8618

    public void UpdateBasicData(
        string? firstName,
        string? lastName,
        string username,
        bool emailNotificationEnabled = false
    )
    {
        FirstName = firstName;
        LastName = lastName;
        Username = username;
        EmailNotificationEnabled = emailNotificationEnabled;
    }

    public void UpdateEmail(string email) => Email = email;

    public void UpdatePassword(string? password) => Password = password;
}