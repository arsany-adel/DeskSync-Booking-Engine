using DeskSync.Api.Configuration;
using DeskSync.Api.Data;
using DeskSync.Api.Entities;
using DeskSync.Api.Services.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;

namespace DeskSync.Api.Services;

public class NotificationWorkerService(
    AppDbContext context,
    IBackgroundJobClient backgroundJobClient,
    IClock clock,
    IOptions<WorkerSettingsOptions> workerOptions,
    ILogger<NotificationWorkerService> logger) : INotificationWorkerService
{
    private readonly AppDbContext _context = context;
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;
    private readonly IClock _clock = clock;
    private readonly ILogger<NotificationWorkerService> _logger = logger;
    private readonly int _batchSize = workerOptions.Value.DefaultBatchSize;
    private readonly int _reminderWindowMinutes = workerOptions.Value.ReminderWindowMinutes;

    public async Task ProcessReservationRemindersAsync(CancellationToken cancellationToken)
    {
        // Create a set to track skipped reservations across batches
        var skipped = new HashSet<Guid>();

        while (true)
        {
            var processed = await ProcessReminderBatchAsync(skipped, cancellationToken);

            if (processed == 0)
                return;
        }
    }

    private async Task<int> ProcessReminderBatchAsync(HashSet<Guid> skipped, CancellationToken cancellationToken)
    {
        var now = _clock.GetCurrentInstant();
        var windowEnd = now.Plus(Duration.FromMinutes(_reminderWindowMinutes));

        var dueReservations = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.UtcStartTime > now && r.UtcStartTime <= windowEnd)
            .Where(r => !_context.Notifications.Any(n =>
                n.ReservationId == r.Id &&
                n.Type == NotificationType.ReservationReminder))
            .Where(r => !skipped.Contains(r.Id)) // Exclude poisoned rows
            .OrderBy(r => r.UtcStartTime)
            .Select(r => new
            {
                ReservationId = r.Id,
                r.UserId,
                RoomName = r.Room.Name,
                r.UtcStartTime,
                HasEmail = r.User.Email != null && r.User.Email != "",
                r.User.EmailNotificationEnabled,
            })
            .Take(_batchSize)
            .ToListAsync(cancellationToken);

        if (dueReservations.Count == 0)
            return 0;

        _logger.LogInformation("Processing batch of {Count} reservation reminders.", dueReservations.Count);

        var notifications = new List<Notification>(dueReservations.Count);

        foreach (var r in dueReservations)
        {
            var minutes = Math.Max(1, (int)Math.Ceiling((r.UtcStartTime - now).TotalMinutes));
            var unit = minutes == 1 ? "minute" : "minutes";

            var title = "Upcoming Reservation Reminder";
            var message = $"Your reservation for room '{r.RoomName}' starts in about {minutes} {unit}.";

            notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = r.UserId,
                ReservationId = r.ReservationId,
                Title = title,
                Message = message,
                Type = NotificationType.ReservationReminder,
                IsRead = false,
                CreatedAt = now,
            });
        }

        try
        {
            await _context.Notifications.AddRangeAsync(notifications, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _context.ChangeTracker.Clear();
            _logger.LogWarning(ex, "Batch insert failed, retrying items individually.");
            notifications = await InsertOneByOneAsync(notifications, skipped, cancellationToken);
        }

        // Only enqueue for users who have email enabled
        var emailableReservations = dueReservations
            .Where(d => d.EmailNotificationEnabled && d.HasEmail)
            .Select(d => d.ReservationId)
            .ToHashSet();

        foreach (var notification in notifications.Where(n => emailableReservations.Contains(n.ReservationId!.Value)))
        {
            _backgroundJobClient.Enqueue<IEmailService>(s => s.SendNotificationEmailAsync(notification.Id, CancellationToken.None));
        }

        return dueReservations.Count; // Return the count we attempted, so the loop moves forward
    }

    private async Task<List<Notification>> InsertOneByOneAsync(
        List<Notification> items, 
        HashSet<Guid> skipped, 
        CancellationToken ct)
    {
        var saved = new List<Notification>();
        foreach (var n in items)
        {
            try
            {
                _context.Notifications.Add(n);
                await _context.SaveChangesAsync(ct);
                saved.Add(n);
            }
            catch (DbUpdateException ex)
            {
                _context.ChangeTracker.Clear();
                skipped.Add(n.ReservationId!.Value);
                _logger.LogWarning(ex, "Skipping reminder for reservation {ReservationId}.", n.ReservationId);
            }
        }
        return saved;
    }
}