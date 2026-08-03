using System.Text.RegularExpressions;

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

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

    public void UpdateEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new ArgumentException("Email cannot be empty.");

        if (!EmailRegex.IsMatch(newEmail))
            throw new ArgumentException("Invalid email format.");

        Email = newEmail;
    }

    public void UpdatePassword(string? password) => Password = password;
}