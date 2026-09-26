using StarterApp.Application.Common;

namespace StarterApp.Application.Interfaces;

/// <summary>
/// Service for sending emails.
/// </summary>
public interface IEmailService
{
    Task<OperationResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendHtmlFormattedEmailAsync(string recipientEmail, string subject, string body, CancellationToken cancellationToken = default);
}
