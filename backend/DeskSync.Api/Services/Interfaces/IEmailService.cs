using ErrorOr;

namespace DeskSync.Api.Services.Interfaces;

public interface IEmailService
{
    Task<ErrorOr<Success>> SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
}