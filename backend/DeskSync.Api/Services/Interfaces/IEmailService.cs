using ErrorOr;

namespace DeskSync.Api.Services.Interfaces;

public interface IEmailService
{
    Task<ErrorOr<Success>> SendNotificationEmailAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);
}