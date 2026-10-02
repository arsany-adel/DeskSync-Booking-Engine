using DeskSync.Api.Services.Interfaces;
using Hangfire;

namespace DeskSync.Api.Configuration;

public static class BackgroundJobRegistration
{
    public static WebApplication MapRecurringJobs(this WebApplication app)
    {
        RecurringJob.AddOrUpdate<ITzdbSyncService>(
            "tzdb-daily-sync",
            service => service.SyncReservationsAsync(CancellationToken.None),
            Cron.Daily(2));

        RecurringJob.AddOrUpdate<INotificationWorkerService>(
            "process-reservation-reminders",
            service => service.ProcessReservationRemindersAsync(CancellationToken.None),
            "*/5 * * * *");

        RecurringJob.AddOrUpdate<INotificationWorkerService>(
            "process-bookmark-updates",
            service => service.ProcessBookmarkUpdatesAsync(CancellationToken.None),
            Cron.Hourly);

        return app;
    }
}