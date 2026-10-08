using DeskSync.Api.Data;
using DeskSync.Api.Entities;
using DeskSync.Api.Services.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace DeskSync.Api.Services;

public class NotificationWorkerService(
    AppDbContext context,
    IBackgroundJobClient backgroundJobClient,
    IClock clock,
    ILogger<NotificationWorkerService> logger) : INotificationWorkerService
{
    private readonly AppDbContext _context = context;
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;
    private readonly IClock _clock = clock;
    private readonly ILogger<NotificationWorkerService> _logger = logger;

    // Critical #1: window must be at least as wide as the cron interval.
    // Cron runs every 5 min; window = 15 min covers it plus a 3x safety margin.
    private const int ReminderWindowMinutes = 15;

    // Important: batch limit to bound memory per run.
    private const int BatchSize = 500;

    public async Task ProcessReservationRemindersAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var processed = await ProcessReminderBatchAsync(cancellationToken);

            if (processed == 0)
                return;
        }
    }

    private async Task<int> ProcessReminderBatchAsync(CancellationToken cancellationToken)
    {
        var now = _clock.GetCurrentInstant();
        var windowEnd = now.Plus(Duration.FromMinutes(ReminderWindowMinutes));

        // Important: no full entities, just the fields needed. AsNoTracking for read-only.
        var dueReservations = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.UtcStartTime > now && r.UtcStartTime <= windowEnd)
            .Where(r => !_context.Notifications.Any(n =>
                n.ReservationId == r.Id &&
                n.Type == NotificationType.ReservationReminder))
            .OrderBy(r => r.UtcStartTime)
            .Select(r => new
            {
                ReservationId = r.Id,
                r.UserId,
                RoomName = r.Room.Name,
                r.UtcStartTime,
                Email = r.User.Email,
                r.User.EmailNotificationEnabled,
            })
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (dueReservations.Count == 0)
            return 0;

        _logger.LogInformation(
            "Processing batch of {Count} reservation reminders (window: {Start} → {End}).",
            dueReservations.Count, now, windowEnd);

        var notifications = new List<Notification>(dueReservations.Count);

        foreach (var r in dueReservations)
        {
            // Critical #7 + Important: compute remaining minutes instead of hardcoding 15.
            var minutes = Math.Max(
                1,
                (int)Math.Ceiling((r.UtcStartTime - now).TotalMinutes));

            var title = "Upcoming Reservation Reminder";
            var message =
                $"Your reservation for room '{r.RoomName}' starts in about {minutes} minutes.";

            notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = r.UserId,
                ReservationId = r.ReservationId, // Critical #2: dedup key
                Title = title,
                Message = message,               // stored as plain text
                Type = NotificationType.ReservationReminder,
                IsRead = false,
                CreatedAt = now,
            });
        }

        // Critical #5: save once per batch, catch unique-index violations.
        try
        {
            await _context.Notifications.AddRangeAsync(notifications, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // A concurrent run inserted the same (ReservationId, Type) — unique index tripped.
            // Clear the tracker so subsequent SaveChanges calls aren't poisoned.
            _logger.LogWarning(
                ex,
                "Batch insert hit a unique-index conflict. Clearing tracker and continuing.");

            _context.ChangeTracker.Clear();

            // The whole batch failed; break out and let the next cron run pick up whatever's left.
            return 0;
        }

        // Critical #4: enqueue one email job per notification, carrying only the id.
        foreach (var notification in notifications)
        {
            _backgroundJobClient.Enqueue<IEmailService>(
                service => service.SendNotificationEmailAsync(
                    notification.Id,
                    CancellationToken.None));
        }

        return notifications.Count;
    }
}