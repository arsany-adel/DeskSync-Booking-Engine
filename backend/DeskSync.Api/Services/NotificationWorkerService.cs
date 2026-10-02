using DeskSync.Api.Data;
using DeskSync.Api.Entities;
using DeskSync.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using ErrorOr;

namespace DeskSync.Api.Services;

public class NotificationWorkerService(
    AppDbContext context,
    IEmailService emailService,
    IClock clock,
    ILogger<NotificationWorkerService> logger) : INotificationWorkerService
{
    private readonly AppDbContext _context = context;
    private readonly IEmailService _emailService = emailService;
    private readonly IClock _clock = clock;
    private readonly ILogger<NotificationWorkerService> _logger = logger;

    public async Task ProcessReservationRemindersAsync(CancellationToken cancellationToken)
    {
        var now = _clock.GetCurrentInstant();
        
        var reminderWindowStart = now.Plus(Duration.FromMinutes(14));
        var reminderWindowEnd = now.Plus(Duration.FromMinutes(16));
        var oneHourAgo = now.Minus(Duration.FromHours(1)); // Pre-computed for SQL parameterization

        var upcomingReservations = await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Room)
            .Where(r => r.UtcStartTime >= reminderWindowStart && r.UtcStartTime <= reminderWindowEnd)
            .Where(r => !_context.Notifications.Any(n => 
                n.UserId == r.UserId && 
                n.Type == NotificationType.ReservationReminder && 
                n.CreatedAt >= oneHourAgo))
            .ToListAsync(cancellationToken);

        if (!upcomingReservations.Any())
            return;

        _logger.LogInformation("Processing {Count} reservation reminders.", upcomingReservations.Count);

        foreach (var reservation in upcomingReservations)
        {
            var title = "Upcoming Reservation Reminder";
            var message = $"Your reservation for room '{reservation.Room.Name}' starts in 15 minutes.";

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = reservation.UserId,
                Title = title,
                Message = message,
                Type = NotificationType.ReservationReminder,
                IsRead = false,
                CreatedAt = now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            if (reservation.User.EmailNotificationEnabled && !string.IsNullOrWhiteSpace(reservation.User.Email))
            {
                var emailResult = await _emailService.SendEmailAsync(
                    reservation.User.Email,
                    title,
                    message,
                    cancellationToken
                );

                if (emailResult.IsError)
                {
                    _logger.LogError("Failed to send reminder email to {Email}. Errors: {Errors}", 
                        reservation.User.Email, 
                        string.Join(", ", emailResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    public async Task ProcessBookmarkUpdatesAsync(CancellationToken cancellationToken)
    {
        var now = _clock.GetCurrentInstant();
        var oneDayAgo = now.Minus(Duration.FromDays(1)); // Avoid repeating daily bookmark alerts

        var bookmarkedRooms = await _context.Bookmarks
            .Include(b => b.User)
            .Include(b => b.Room)
            .Where(b => !_context.Notifications.Any(n =>
                n.UserId == b.UserId &&
                n.Type == NotificationType.BookmarkUpdate &&
                n.CreatedAt >= oneDayAgo))
            .ToListAsync(cancellationToken);

        if (!bookmarkedRooms.Any())
            return;

        _logger.LogInformation("Processing {Count} bookmark update notifications.", bookmarkedRooms.Count);

        foreach (var bookmark in bookmarkedRooms)
        {
            var title = "Bookmarked Room Status Update";
            var message = $"Room '{bookmark.Room.Name}' that you bookmarked is available for reservations.";

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = bookmark.UserId,
                Title = title,
                Message = message,
                Type = NotificationType.BookmarkUpdate,
                IsRead = false,
                CreatedAt = now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            if (bookmark.User.EmailNotificationEnabled && !string.IsNullOrWhiteSpace(bookmark.User.Email))
            {
                var emailResult = await _emailService.SendEmailAsync(
                    bookmark.User.Email,
                    title,
                    message,
                    cancellationToken
                );

                if (emailResult.IsError)
                {
                    _logger.LogError("Failed to send bookmark email to {Email}. Errors: {Errors}",
                        bookmark.User.Email,
                        string.Join(", ", emailResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}