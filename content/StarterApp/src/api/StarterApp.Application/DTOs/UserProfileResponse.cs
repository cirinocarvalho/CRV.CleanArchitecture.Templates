namespace StarterApp.Application.DTOs;

/// <summary>
/// Combined profile DTO that links an authenticated identity user with their business user data.
/// </summary>
public record UserProfileResponse
{
    public string IdentityUserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public IList<string> Roles { get; init; } = [];
}
