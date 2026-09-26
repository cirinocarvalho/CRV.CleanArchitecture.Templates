using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StarterApp.Application.Interfaces;

namespace StarterApp.Infrastructure.Identity;

/// <summary>
/// Role manager service for managing application roles.
/// Wraps ASP.NET Core Identity RoleManager for role operations.
/// </summary>
public class RoleManagerService(RoleManager<ApplicationRole> roleManager) : IRoleManagerService
{
    /// <summary>
    /// Creates a new role in the system.
    /// </summary>
    /// <param name="roleName">Name of the role to create</param>
    /// <returns>True if role creation successful, false if role already exists or creation fails</returns>
    public async Task<bool> CreateRoleAsync(string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
            return false;

        var result = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
        return result.Succeeded;
    }

    /// <summary>
    /// Retrieves all available roles in the system.
    /// </summary>
    /// <returns>List of all role names</returns>
    public async Task<List<string>> GetAllRolesAsync()
    {
        return await roleManager.Roles
            .Select(r => r.Name!)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if a role with given name exists in the system.
    /// </summary>
    /// <param name="roleName">Name of the role to check</param>
    /// <returns>True if role exists, false otherwise</returns>
    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await roleManager.RoleExistsAsync(roleName);
    }
}