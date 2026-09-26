namespace StarterApp.Application.DTOs;

/// <summary>
/// Admin view of a user: identity, roles, and assigned company.
/// </summary>
public record UserListItem
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public IList<string> Roles { get; init; } = [];
    public int? CompanyId { get; init; }
    public string? CompanyName { get; init; }
    public bool IsActive { get; init; }
}
