namespace StarterApp.Application.DTOs;

/// <summary>
/// Change password request DTO for authenticated users.
/// Requires current password for verification.
/// </summary>
public record ChangePasswordRequest
{
    /// <summary>
    /// User's email address for identification.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// User's current password for verification.
    /// Required for security validation.
    /// </summary>
    public string CurrentPassword { get; init; } = string.Empty;

    /// <summary>
    /// New password to set for the user.
    /// </summary>
    public string NewPassword { get; init; } = string.Empty;
}