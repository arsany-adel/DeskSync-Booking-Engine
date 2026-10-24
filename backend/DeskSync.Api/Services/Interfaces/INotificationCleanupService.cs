using Hangfire;

namespace DeskSync.Api.Services.Interfaces;

public interface INotificationCleanupService
{
    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    Task PurgeOldNotificationsAsync(CancellationToken cancellationToken);
}