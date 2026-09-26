using StarterApp.Application.DTOs;

namespace StarterApp.Application.Interfaces;

/// <summary>
/// Role service interface defining role management operations.
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Creates a new role in the system.
    /// </summary>
    /// <param name="request">Role request containing the role name</param>
    /// <returns>True if role creation successful, false otherwise</returns>
    Task<bool> CreateRoleAsync(RoleRequest request);

    /// <summary>
    /// Retrieves all available roles in the system.
    /// </summary>
    /// <returns>List of all role names</returns>
    Task<List<string>> GetAllRolesAsync();
}
