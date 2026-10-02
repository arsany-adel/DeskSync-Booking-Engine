using NodaTime;

namespace DeskSync.Api.Entities;

public enum NotificationType
{
    ReservationReminder,
    BookmarkUpdate,
    System
}

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }

    public Instant CreatedAt { get; set; }
}