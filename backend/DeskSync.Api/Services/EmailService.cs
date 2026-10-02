using System.Net;
using System.Net.Mail;
using DeskSync.Api.Services.Interfaces;
using ErrorOr;

namespace DeskSync.Api.Services;

public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<EmailService> _logger = logger;

    public async Task<ErrorOr<Success>> SendEmailAsync(
        string toEmail, 
        string subject, 
        string body, 
        CancellationToken cancellationToken = default)
    {
        var smtpHost = _configuration["Smtp:Host"];

        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            _logger.LogWarning("SMTP Host is missing in configuration. Skipping email for {ToEmail}.", toEmail);
            
            return Error.Failure(
                code: "Email.ConfigurationMissing", 
                description: "SMTP server host configuration is missing.");
        }

        if (!int.TryParse(_configuration["Smtp:Port"], out var smtpPort))
        {
            smtpPort = 587;
        }

        var smtpUser = _configuration["Smtp:Username"];
        var smtpPass = _configuration["Smtp:Password"];
        var fromAddress = _configuration["Smtp:FromAddress"] ?? "noreply@desksync.com";

        try
        {
            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress, "DeskSync"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage, cancellationToken);
            
            return Result.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}.", toEmail);

            return Error.Failure(
                code: "Email.SendFailed", 
                description: $"An error occurred while sending email: {ex.Message}");
        }
    }
}