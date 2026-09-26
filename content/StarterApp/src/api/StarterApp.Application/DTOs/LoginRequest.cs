namespace StarterApp.Application.DTOs;

/// <summary>
/// Login request DTO containing user credentials.
/// Used for user authentication requests.
/// </summary>
public record LoginRequest
{
    /// <summary>
    /// User's email address for authentication.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// User's password for authentication.
    /// </summary>
    public string Password { get; init; } = string.Empty;
}

