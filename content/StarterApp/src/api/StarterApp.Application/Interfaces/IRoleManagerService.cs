namespace StarterApp.Application.Interfaces;

/// <summary>
/// Role manager service interface for managing application roles.
/// </summary>
public interface IRoleManagerService
{
    /// <summary>
    /// Creates a new role in the system.
    /// </summary>
    /// <param name="roleName">Name of the role to create</param>
    /// <returns>True if role creation successful, false otherwise</returns>
    Task<bool> CreateRoleAsync(string roleName);

    /// <summary>
    /// Retrieves all available roles in the system.
    /// </summary>
    /// <returns>List of all role names</returns>
    Task<List<string>> GetAllRolesAsync();

    /// <summary>
    /// Checks if a role with given name exists in the system.
    /// </summary>
    /// <param name="roleName">Name of the role to check</param>
    /// <returns>True if role exists, false otherwise</returns>
    Task<bool> RoleExistsAsync(string roleName);
}