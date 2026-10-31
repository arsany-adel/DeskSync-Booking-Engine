using ErrorOr;
using Hangfire;

namespace DeskSync.Api.Services.Interfaces;

public interface IEmailService
{
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 120, 300 })]
    [Queue("email")]
    Task SendNotificationEmailAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<Success>> SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string plainTextBody,
        CancellationToken cancellationToken = default);
}