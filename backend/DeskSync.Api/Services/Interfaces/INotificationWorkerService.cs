namespace DeskSync.Api.Services.Interfaces;
using Hangfire;

public interface INotificationWorkerService
{
    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    Task ProcessReservationRemindersAsync(CancellationToken cancellationToken);
}