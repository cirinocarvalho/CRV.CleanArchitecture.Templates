namespace StarterApp.Application.DTOs;

/// <summary>
/// Reset password request DTO for completing password reset flow.
/// Used with a valid reset token from password recovery email.
/// </summary>
public record ResetPasswordRequest
{
    /// <summary>
    /// Email address of the user account to reset password for.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Password reset token sent via email.
    /// Validates that password reset request is legitimate.
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// New password to set for the user account.
    /// </summary>
    public string NewPassword { get; init; } = string.Empty;
}