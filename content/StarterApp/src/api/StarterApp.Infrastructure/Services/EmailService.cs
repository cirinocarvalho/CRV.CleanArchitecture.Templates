using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarterApp.Application.Interfaces;
using StarterApp.Application.Common;

namespace StarterApp.Infrastructure.Services;

/// <summary>
/// SMTP-based email service implementation.
/// </summary>
/// <param name="settings">SMTP configuration settings.</param>
/// <param name="logger">Logger instance.</param>
public class EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger) : IEmailService
{
    private readonly EmailSettings _settings = settings.Value;

    public async Task<OperationResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var result = new OperationResult { Success = false };

        try
        {
            using var message = new MailMessage(_settings.FromAddress, to, subject, body) { IsBodyHtml = true };
            using var client = BuildSmtpClient();
            await client.SendMailAsync(message, cancellationToken);
            result.Success = true;
        }
        catch (Exception ex)
        {
            // The recipient address is personal data and (for account emails)
            // user-supplied, so it is deliberately kept out of the log. The
            // exception carries the SMTP failure detail needed for diagnosis.
            logger.LogError(ex, "Failed to send email with subject {Subject}.", LogSanitizer.Sanitize(subject));
            result.AddMessage(ex.Message);
        }

        return result;
    }

    public async Task SendHtmlFormattedEmailAsync(string recipientEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(recipientEmail));

            using var client = BuildSmtpClient();
            await client.SendMailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            // See SendEmailAsync: the address is PII, so only the subject is logged.
            logger.LogError(ex, "Failed to send HTML email with subject {Subject}.", LogSanitizer.Sanitize(subject));
        }
    }

    private SmtpClient BuildSmtpClient()
    {
        var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.EnableSsl
        };

        if (!string.IsNullOrEmpty(_settings.SmtpUser))
        {
            client.Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPassword);
        }

        return client;
    }
}
