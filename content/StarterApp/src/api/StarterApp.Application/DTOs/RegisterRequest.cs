namespace StarterApp.Application.DTOs;

/// <summary>
/// User registration request DTO containing new user information.
/// Used for creating new user accounts.
/// </summary>
public record RegisterRequest
{
    /// <summary>
    /// Email address for the new user account.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Password for the new user account.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Full name of the new user.
    /// </summary>
    public string FullName { get; init; } = string.Empty;
}
