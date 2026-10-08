using DeskSync.Api.Configuration;
using DeskSync.Api.Constants;
using DeskSync.Api.Data;
using DeskSync.Api.Services.Interfaces;
using ErrorOr;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace DeskSync.Api.Services;

public class EmailService(
    IOptions<SmtpSettingsOptions> smtpOptions,
    AppDbContext context,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly SmtpSettingsOptions _smtp = smtpOptions.Value;
    private readonly AppDbContext _context = context;
    private readonly ILogger<EmailService> _logger = logger;

    private static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(15);

    public async Task<ErrorOr<Success>> SendNotificationEmailAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications
            .AsNoTracking()
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken);

        if (notification is null)
        {
            // Job referenced a notification that no longer exists. Nothing to send.
            _logger.LogWarning(
                "Email job skipped: notification {NotificationId} not found.",
                notificationId);

            return Result.Success;
        }

        if (!notification.User.EmailNotificationEnabled
            || string.IsNullOrWhiteSpace(notification.User.Email))
        {
            // Preference disabled between enqueue and send. Skip silently.
            return Result.Success;
        }

        try
        {
            var message = BuildMessage(
                toEmail: notification.User.Email,
                subject: notification.Title,
                title: notification.Title,
                body: notification.Message);

            using var client = new SmtpClient();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(SendTimeout);

            var socketOptions = _smtp.UseStartTls
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.SslOnConnect;

            await client.ConnectAsync(_smtp.Host, _smtp.Port, socketOptions, cts.Token);

            if (!string.IsNullOrWhiteSpace(_smtp.Username)
                && !string.IsNullOrWhiteSpace(_smtp.Password))
            {
                await client.AuthenticateAsync(_smtp.Username, _smtp.Password, cts.Token);
            }

            await client.SendAsync(message, cts.Token);
            await client.DisconnectAsync(quit: true, cts.Token);

            return Result.Success;
        }
        catch (OperationCanceledException)
        {
            // Let Hangfire see the cancellation and stop retrying this job.
            throw;
        }
        catch (Exception ex)
        {
            // Log full detail, return a generic message (no SMTP internals leak to the API).
            _logger.LogError(
                ex,
                "Failed to send email for notification {NotificationId}.",
                notificationId);

            return DomainErrors.Email.SendFailed;
        }
    }

    private MimeMessage BuildMessage(
        string toEmail,
        string subject,
        string title,
        string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("DeskSync", _smtp.FromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        // Unsubscribe header — points at the user's notification settings page.
        message.Headers.Add(
            HeaderId.ListUnsubscribe,
            "<https://app.desksync.com/settings/notifications>");

        var plainText = $"{title}\n\n{body}\n\n---\nDeskSync";
        var html = $@"<html><body>
<h2>{System.Net.WebUtility.HtmlEncode(title)}</h2>
<p>{System.Net.WebUtility.HtmlEncode(body)}</p>
<hr/>
<p><small>DeskSync</small></p>
</body></html>";

        var bodyBuilder = new BodyBuilder
        {
            TextBody = plainText,
            HtmlBody = html,
        };
        message.Body = bodyBuilder.ToMessageBody();

        return message;
    }
}