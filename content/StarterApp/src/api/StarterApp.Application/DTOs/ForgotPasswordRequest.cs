namespace StarterApp.Application.DTOs;

/// <summary>
/// Forgot password request DTO for initiating password reset flow.
/// Used when user needs to reset forgotten password.
/// </summary>
public record ForgotPasswordRequest
{
    /// <summary>
    /// Email address of the user account to reset password for.
    /// </summary>
    public string Email { get; init; } = string.Empty;
}