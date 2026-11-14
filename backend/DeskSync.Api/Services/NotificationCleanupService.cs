using DeskSync.Api.Configuration;
using DeskSync.Api.Data;
using DeskSync.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;

namespace DeskSync.Api.Services;

public class NotificationCleanupService(
    AppDbContext context,
    IClock clock,
    IOptions<WorkerSettingsOptions> workerOptions,
    ILogger<NotificationCleanupService> logger) : INotificationCleanupService
{
    private readonly AppDbContext _context = context;
    private readonly IClock _clock = clock;
    private readonly ILogger<NotificationCleanupService> _logger = logger;
    private readonly int _retentionDays = workerOptions.Value.NotificationRetentionDays;

    public async Task PurgeOldNotificationsAsync(CancellationToken cancellationToken)
    {
        var cutoff = _clock.GetCurrentInstant().Minus(Duration.FromDays(_retentionDays));

        _logger.LogInformation(
            "Purging read notifications older than {Cutoff} (retention: {Days} days).",
            cutoff, _retentionDays);

        // ExecuteDeleteAsync issues a single DELETE ... WHERE statement, no tracking.
        var deleted = await _context.Notifications
            .Where(n => n.IsRead && n.CreatedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0)
        {
            _logger.LogInformation("Purged {Count} old notifications.", deleted);
        }
    }
}