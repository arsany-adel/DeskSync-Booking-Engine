using DeskSync.Api.Services.Interfaces;
using Hangfire;

namespace DeskSync.Api.Configuration;

public static class BackgroundJobRegistration
{
    public static WebApplication UseRecurringJobs(this WebApplication app)
    {
        var jobManager = app.Services.GetRequiredService<IRecurringJobManager>();

        // Remove retired jobs so a stale entry doesn't keep firing.
        jobManager.RemoveIfExists("process-bookmark-updates");

        jobManager.AddOrUpdate<ITzdbSyncService>(
            "tzdb-daily-sync",
            service => service.SyncReservationsAsync(CancellationToken.None),
            Cron.Daily(2));

        jobManager.AddOrUpdate<INotificationWorkerService>(
            "process-reservation-reminders",
            service => service.ProcessReservationRemindersAsync(CancellationToken.None),
            "*/5 * * * *");

        // bookmark job is DISABLED until Room.Status change events exist.
        // Re-enable when bookmark notifications become event-driven:
        //   jobManager.AddOrUpdate<INotificationWorkerService>(
        //       "process-bookmark-updates",
        //       service => service.ProcessBookmarkUpdatesAsync(CancellationToken.None),
        //       Cron.Daily(9));

        return app;
    }
}