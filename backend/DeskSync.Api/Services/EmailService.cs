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
using NodaTime;

namespace DeskSync.Api.Services;

public class EmailService(
    IOptions<SmtpSettingsOptions> smtpOptions,
    AppDbContext context,
    IClock clock,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly SmtpSettingsOptions _smtp = smtpOptions.Value;
    private readonly AppDbContext _context = context;
    private readonly IClock _clock = clock;
    private readonly ILogger<EmailService> _logger = logger;

    private static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(15);

    public async Task<ErrorOr<Success>> SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string plainTextBody,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("DeskSync", _smtp.FromAddress));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                TextBody = plainTextBody,
                HtmlBody = htmlBody,
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            client.CheckCertificateRevocation = false; 

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(SendTimeout);

            var socketOptions = _smtp.UseStartTls
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.SslOnConnect;

            await client.ConnectAsync(_smtp.Host, _smtp.Port, socketOptions, cts.Token);

            if (!string.IsNullOrWhiteSpace(_smtp.Username) && !string.IsNullOrWhiteSpace(_smtp.Password))
            {
                await client.AuthenticateAsync(_smtp.Username, _smtp.Password, cts.Token);
            }

            await client.SendAsync(message, cts.Token);

            try { await client.DisconnectAsync(quit: true, CancellationToken.None); } catch { }

            return Result.Success;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}.", toEmail);
            return DomainErrors.Email.SendFailed;
        }
    }

    public async Task SendNotificationEmailAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        // Feedback #4: Projection to avoid loading PasswordHash
        var notification = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.Id == notificationId)
            .Select(n => new
            {
                n.Title,
                n.Message,
                n.CreatedAt,
                n.IsRead,
                n.EmailSentAt,
                UserEmail = n.User.Email,
                UserEmailNotificationEnabled = n.User.EmailNotificationEnabled,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (notification is null)
        {
            _logger.LogWarning("Email job skipped: notification {NotificationId} not found.", notificationId);
            return; // Dequeue permanently.
        }

        // Idempotency check: skip if already sent
        if (notification.EmailSentAt != null)
        {
            return; // Dequeue permanently.
        }

        // TTL check: drop if the notification is too old (e.g., stale retry)
        var now = _clock.GetCurrentInstant();
        if (now - notification.CreatedAt > Duration.FromMinutes(15))
        {
            _logger.LogWarning("Email job skipped: notification {NotificationId} is too old.", notificationId);
            return; // Dequeue permanently.
        }

        // Skip if the user already read the notification in-app
        if (notification.IsRead)
        {
            _logger.LogInformation("Email job skipped: notification {NotificationId} was already read.", notificationId);
            return; // Dequeue permanently.
        }

        if (!notification.UserEmailNotificationEnabled || string.IsNullOrWhiteSpace(notification.UserEmail))
        {
            return; // User disabled preferences. Dequeue permanently.
        }

        try
        {
            var message = BuildMessage(
                toEmail: notification.UserEmail, // <--- Fixed here
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

            if (!string.IsNullOrWhiteSpace(_smtp.Username) && !string.IsNullOrWhiteSpace(_smtp.Password))
            {
                await client.AuthenticateAsync(_smtp.Username, _smtp.Password, cts.Token);
            }

            await client.SendAsync(message, cts.Token);

            // Mark as sent immediately to prevent duplicate sends on retry
            await _context.Notifications
                .Where(n => n.Id == notificationId)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(n => n.EmailSentAt, _clock.GetCurrentInstant()), 
                    CancellationToken.None);

            try 
            { 
                await client.DisconnectAsync(quit: true, CancellationToken.None); 
            } 
            catch 
            { 
                // Ignore disconnect failures. The email was already sent and the DB is updated.
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // Real shutdown/cancel. Let Hangfire see it.
        }
        catch (Exception ex) when (ex is ParseException or
                                   SmtpCommandException { ErrorCode: SmtpErrorCode.RecipientNotAccepted })
        {
            _logger.LogError(ex, "Permanent email failure for notification {NotificationId}.", notificationId);
            return; // Permanent failure (e.g., bad address). Do not retry.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Transient email failure for notification {NotificationId}.", notificationId);
            throw; // Transient failure (e.g., timeout, network drop). Hangfire will retry.
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
            $"<{_smtp.AppBaseUrl}/settings/notifications>");

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