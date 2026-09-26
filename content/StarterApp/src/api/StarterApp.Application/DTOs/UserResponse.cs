namespace StarterApp.Application.DTOs;

/// <summary>
/// DTO for user company/email mapping.
/// </summary>
public record UserResponse
{
    public string Company { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
