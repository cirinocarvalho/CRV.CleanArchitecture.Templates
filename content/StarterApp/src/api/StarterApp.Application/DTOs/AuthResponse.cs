namespace StarterApp.Application.DTOs;

/// <summary>
/// Authentication response DTO containing JWT token and user information.
/// Returned after successful authentication.
/// </summary>
public record AuthResponse
{
    /// <summary>
    /// JWT bearer token for subsequent authenticated requests.
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Authenticated user's email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// List of roles assigned to the authenticated user.
    /// </summary>
    public IList<string> Roles { get; init; } = [];
}