using StarterApp.Application.DTOs;
using StarterApp.Application.Interfaces;

namespace StarterApp.Application.Services;

/// <summary>
/// Role service implementing role management operations.
/// Delegates to role manager service for actual role operations.
/// </summary>
public class RoleService(IRoleManagerService roleManagerService) : IRoleService
{
    /// <summary>
    /// Creates a new role in the system.
    /// </summary>
    /// <param name="request">Role request containing the role name</param>
    /// <returns>True if role creation successful, false otherwise</returns>
    public async Task<bool> CreateRoleAsync(RoleRequest request)
    {
        return await roleManagerService.CreateRoleAsync(request.RoleName);
    }

    /// <summary>
    /// Retrieves all available roles in the system.
    /// </summary>
    /// <returns>List of all role names</returns>
    public async Task<List<string>> GetAllRolesAsync()
    {
        return await roleManagerService.GetAllRolesAsync();
    }
}
