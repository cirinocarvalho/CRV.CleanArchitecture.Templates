namespace StarterApp.Application.DTOs;

/// <summary>
/// Role request DTO for creating or managing user roles.
/// Used in role management operations by administrators.
/// </summary>
public record RoleRequest
{
    /// <summary>
    /// Name of the role to create or manage.
    /// Must be unique within the system.
    /// </summary>
    public string RoleName { get; init; } = string.Empty;
}

