namespace StarterApp.Application.Interfaces;

/// <summary>
/// Sends transactional account emails (registration received, password changed,
/// password reset). Implementations are best-effort: a delivery failure is logged
/// but never propagated, so it cannot break the underlying auth operation.
/// </summary>
public interface IAccountEmailSender
{
    /// <summary>
    /// Notifies a newly registered user that their account was created and is
    /// pending administrator approval before they can sign in.
    /// </summary>
    Task SendRegistrationReceivedAsync(string email, string fullName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the configured administrator address that a new account was
    /// registered and is waiting for company assignment and activation. Does
    /// nothing when no admin address is configured.
    /// </summary>
    Task SendNewRegistrationToAdminAsync(string email, string fullName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies a user that an administrator has activated their account and they
    /// can now sign in.
    /// </summary>
    Task SendAccountActivatedAsync(string email, string fullName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirms to a user that their account password was just changed.
    /// </summary>
    Task SendPasswordChangedAsync(string email, string fullName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password-reset link containing the supplied reset token.
    /// </summary>
    Task SendPasswordResetAsync(string email, string fullName, string resetToken, CancellationToken cancellationToken = default);
}
