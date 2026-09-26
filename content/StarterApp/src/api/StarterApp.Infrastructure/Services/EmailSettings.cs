namespace StarterApp.Infrastructure.Services;

/// <summary>
/// Configuration settings for SMTP email delivery.
/// Bound from the "Email" configuration section.
/// </summary>
public class EmailSettings
{
    public const string SectionName = "Email";

    /// <summary>
    /// SMTP host name.
    /// </summary>
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>
    /// SMTP port (typically 25, 465, or 587).
    /// </summary>
    public int SmtpPort { get; set; } = 25;

    /// <summary>
    /// Whether TLS/SSL is required.
    /// </summary>
    public bool EnableSsl { get; set; }

    /// <summary>
    /// SMTP user name (leave empty for anonymous relay).
    /// </summary>
    public string? SmtpUser { get; set; }

    /// <summary>
    /// SMTP password.
    /// </summary>
    public string? SmtpPassword { get; set; }

    /// <summary>
    /// Default sender (From) address.
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// No-reply sender address for automated mail.
    /// </summary>
    public string? NoReplyAddress { get; set; }

    /// <summary>
    /// Public base URL of the front-end app (e.g. "https://app.example.com/starterapp").
    /// Used to build links in outgoing mail. No trailing slash required.
    /// </summary>
    public string? AppBaseUrl { get; set; }

    /// <summary>
    /// Front-end route that consumes a password-reset token. Combined with
    /// <see cref="AppBaseUrl"/> to build the reset link.
    /// </summary>
    public string PasswordResetPath { get; set; } = "/forgot-password";

    /// <summary>
    /// Address shown to recipients for questions/feedback. Falls back to
    /// <see cref="FromAddress"/> when not set.
    /// </summary>
    public string? SupportEmail { get; set; }

    /// <summary>
    /// Address notified when a new user registers and is awaiting approval.
    /// Accepts several addresses separated by ';' or ','. Leave empty to disable
    /// the notification.
    /// </summary>
    public string? AdminNotificationEmail { get; set; }
}
