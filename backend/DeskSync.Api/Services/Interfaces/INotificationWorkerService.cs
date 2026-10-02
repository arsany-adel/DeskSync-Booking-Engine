namespace DeskSync.Api.Services.Interfaces;

public interface INotificationWorkerService
{
    Task ProcessReservationRemindersAsync(CancellationToken cancellationToken);
    Task ProcessBookmarkUpdatesAsync(CancellationToken cancellationToken);
}