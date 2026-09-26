using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarterApp.Application.Interfaces;

namespace StarterApp.Infrastructure.Services;

/// <summary>
/// Composes branded account emails (registration, password change, password reset)
/// and hands them to <see cref="IEmailService"/> for SMTP delivery. All sends are
/// best-effort: failures are logged and swallowed so they never break the auth flow.
/// </summary>
public class AccountEmailSender(
    IEmailService emailService,
    IOptions<EmailSettings> settings,
    ILogger<AccountEmailSender> logger) : IAccountEmailSender
{
    private readonly EmailSettings _settings = settings.Value;

    public async Task SendRegistrationReceivedAsync(string email, string fullName, CancellationToken cancellationToken = default)
    {
        const string subject = "StarterApp — Registration received";
        var body =
            "Thank you for registering for StarterApp. Your account has been created and is " +
            "now <b>pending review</b> by an administrator." +
            "<br /><br />" +
            "Once an administrator assigns your company and activates your account, you will be " +
            "able to sign in. No further action is needed from you right now — we will follow up " +
            "when your account is ready.";

        await SendAsync(email, fullName, subject, body, cancellationToken);
    }

    public async Task SendNewRegistrationToAdminAsync(string email, string fullName, CancellationToken cancellationToken = default)
    {
        var recipients = AdminRecipients();
        if (recipients.Length == 0)
        {
            logger.LogDebug("No Email:AdminNotificationEmail configured — skipping new-registration notification.");
            return;
        }

        const string subject = "StarterApp — New account awaiting approval";
        var adminLink = BuildLink("/admin");
        var body =
            "A new user has registered and is <b>awaiting approval</b>:" +
            "<br /><br />" +
            $"Name: <b>{WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(fullName) ? "(not provided)" : fullName)}</b><br />" +
            $"Email: <b>{WebUtility.HtmlEncode(email)}</b>" +
            "<br /><br />" +
            "The account is inactive and assigned to the \"None\" company until an administrator " +
            "assigns the correct company and activates it." +
            "<br /><br />" +
            $"<a style=\"color: #22BCE5\" href=\"{adminLink}\">Open the admin users panel</a>";

        foreach (var recipient in recipients)
        {
            await SendAsync(recipient, "Administrator", subject, body, cancellationToken);
        }
    }

    public async Task SendAccountActivatedAsync(string email, string fullName, CancellationToken cancellationToken = default)
    {
        const string subject = "StarterApp — Your account is now active";
        var loginLink = BuildLink("/login");
        var body =
            "Good news — an administrator has reviewed and <b>activated</b> your StarterApp " +
            "account. You can now sign in and start using the system." +
            "<br /><br />" +
            $"<a style=\"color: #22BCE5\" href=\"{loginLink}\">Sign in to StarterApp</a>";

        await SendAsync(email, fullName, subject, body, cancellationToken);
    }

    public async Task SendPasswordChangedAsync(string email, string fullName, CancellationToken cancellationToken = default)
    {
        const string subject = "StarterApp — Your password was changed";
        var body =
            $"This is a confirmation that the password for your StarterApp account " +
            $"(<b>{WebUtility.HtmlEncode(email)}</b>) was just changed." +
            "<br /><br />" +
            "If you made this change, no action is needed. If you did <b>not</b> change your " +
            $"password, please contact us immediately at {SupportAddress()}.";

        await SendAsync(email, fullName, subject, body, cancellationToken);
    }

    public async Task SendPasswordResetAsync(string email, string fullName, string resetToken, CancellationToken cancellationToken = default)
    {
        const string subject = "StarterApp — Password reset";
        var resetLink = BuildResetLink(email, resetToken);
        var body =
            "We received a request to reset the password for your StarterApp account. " +
            "Click the link below to set a new password:" +
            "<br /><br />" +
            $"<a style=\"color: #22BCE5\" href=\"{resetLink}\">Reset your password</a>" +
            "<br /><br />" +
            "If you did not request a password reset, you can safely ignore this email — your " +
            "password will remain unchanged.";

        await SendAsync(email, fullName, subject, body, cancellationToken);
    }

    /// <summary>
    /// Builds the front-end reset URL: {AppBaseUrl}{PasswordResetPath}?email=&amp;token=.
    /// Both values are URL-encoded (Identity tokens contain +, /, and =).
    /// </summary>
    private string BuildResetLink(string email, string token) =>
        $"{BuildLink(_settings.PasswordResetPath)}?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

    /// <summary>
    /// Builds a front-end URL from <see cref="EmailSettings.AppBaseUrl"/> and the given path.
    /// </summary>
    private string BuildLink(string relativePath)
    {
        var baseUrl = (_settings.AppBaseUrl ?? string.Empty).TrimEnd('/');
        var path = relativePath.StartsWith('/') ? relativePath : "/" + relativePath;
        return $"{baseUrl}{path}";
    }

    /// <summary>
    /// Splits <see cref="EmailSettings.AdminNotificationEmail"/> into individual
    /// addresses. Returns an empty array when the setting is not configured.
    /// </summary>
    private string[] AdminRecipients() =>
        (_settings.AdminNotificationEmail ?? string.Empty)
            .Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private string SupportAddress() =>
        string.IsNullOrWhiteSpace(_settings.SupportEmail) ? _settings.FromAddress : _settings.SupportEmail;

    private async Task SendAsync(string email, string fullName, string subject, string bodyHtml, CancellationToken cancellationToken)
    {
        try
        {
            var html = BuildBrandedHtml(fullName, bodyHtml);
            await emailService.SendHtmlFormattedEmailAsync(email, subject, html, cancellationToken);
        }
        catch (Exception ex)
        {
            // Never let an email failure surface to the caller — the account
            // action (register / change / reset) has already succeeded.
            // The address is PII supplied by the user, so it stays out of the
            // log; the subject identifies which account email failed.
            logger.LogError(ex, "Failed to send account email {Subject}.", subject);
        }
    }

    /// <summary>
    /// Wraps a message body in the shared branded shell.
    /// </summary>
    private string BuildBrandedHtml(string userName, string bodyHtml) =>
        $$"""
        <!DOCTYPE html>
        <html>
        <head><meta charset="utf-8" /></head>
        <body>
        <div style="border-top: 3px solid #22BCE5">&nbsp;</div>
        <span style="font-family: Arial; font-size: 10pt">
            Hello <b>{{WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(userName) ? "there" : userName)}}</b>,<br /><br />
            {{bodyHtml}}
            <br /><br />
            You are welcome to submit questions and feedback to {{SupportAddress()}}.
            <br /><br />
            Thank you,<br /><br />
            The StarterApp team
        </span>
        </body>
        </html>
        """;
}
