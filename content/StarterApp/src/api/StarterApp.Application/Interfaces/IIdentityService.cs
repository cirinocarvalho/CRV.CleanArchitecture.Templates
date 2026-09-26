using StarterApp.Application.Common;

namespace StarterApp.Application.Interfaces;

/// <summary>
/// Identity service interface defining user identity and authentication operations.
/// Abstracts ASP.NET Core Identity functionality.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Finds a user by email address.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>Identity user if found, null otherwise</returns>
    Task<IIdentityUser?> FindByEmailAsync(string email);

    /// <summary>
    /// Finds a user by their unique identifier.
    /// </summary>
    /// <param name="userId">User's unique identifier</param>
    /// <returns>Identity user if found, null otherwise</returns>
    Task<IIdentityUser?> FindByIdAsync(string userId);

    /// <summary>
    /// Verifies if provided password is correct for the user.
    /// </summary>
    /// <param name="user">The identity user to check password for</param>
    /// <param name="password">Password to verify</param>
    /// <returns>True if password is correct, false otherwise</returns>
    Task<bool> CheckPasswordAsync(IIdentityUser user, string password);

    /// <summary>
    /// Creates a new user in the system.
    /// </summary>
    /// <param name="email">New user's email address</param>
    /// <param name="fullName">New user's full name</param>
    /// <param name="password">New user's password</param>
    /// <returns>True if user creation successful, false otherwise</returns>
    Task<bool> CreateUserAsync(string email, string fullName, string password);

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="roleName">Role name to assign</param>
    /// <returns>True if role assignment successful, false otherwise</returns>
    Task<bool> AssignRoleAsync(string email, string roleName);

    /// <summary>
    /// Retrieves all roles assigned to a user.
    /// </summary>
    /// <param name="user">The identity user</param>
    /// <returns>List of role names assigned to user</returns>
    Task<IList<string>> GetUserRolesAsync(IIdentityUser user);

    /// <summary>
    /// Changes the password for a user.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="currentPassword">Current password for verification</param>
    /// <param name="newPassword">New password to set</param>
    /// <returns>Success flag with any validation messages (e.g. incorrect current password, weak new password)</returns>
    Task<OperationResult> ChangePasswordAsync(string email, string currentPassword, string newPassword);

    /// <summary>
    /// Generates a password reset token for password recovery.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>Password reset token if user found, null otherwise</returns>
    Task<string?> GeneratePasswordResetTokenAsync(string email);

    /// <summary>
    /// Resets user password using a valid reset token.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="token">Password reset token</param>
    /// <param name="newPassword">New password to set</param>
    /// <returns>Success flag with any validation messages (e.g. invalid/expired token, weak new password)</returns>
    Task<OperationResult> ResetPasswordAsync(string email, string token, string newPassword);

    /// <summary>
    /// Returns all users with their assigned roles.
    /// </summary>
    Task<IReadOnlyList<IIdentityUser>> GetAllUsersAsync();

    /// <summary>
    /// Adds a role to the user identified by id. Returns false if the user is not found.
    /// </summary>
    Task<bool> AddToRoleByIdAsync(string userId, string roleName);

    /// <summary>
    /// Removes a role from the user identified by id. Returns false if the user is not found.
    /// </summary>
    Task<bool> RemoveFromRoleByIdAsync(string userId, string roleName);

    /// <summary>
    /// Sets the active state for the user identified by id. Returns false if the user is not found.
    /// </summary>
    Task<bool> SetActiveAsync(string userId, bool isActive);
}