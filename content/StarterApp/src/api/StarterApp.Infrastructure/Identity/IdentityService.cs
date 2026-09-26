using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StarterApp.Application.Common;
using StarterApp.Application.Interfaces;
using StarterApp.Infrastructure.Data.Contexts;

namespace StarterApp.Infrastructure.Identity;

/// <summary>
/// Identity service implementing user identity management operations.
/// Wraps ASP.NET Core Identity UserManager for user and password operations.
/// </summary>
public class IdentityService(
    UserManager<ApplicationUser> userManager,
    AppIdentityDbContext identityDbContext) : IIdentityService
{
    /// <summary>
    /// Finds a user by email address.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>Identity user if found, null otherwise</returns>
    public async Task<IIdentityUser?> FindByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return null;

        var roles = await userManager.GetRolesAsync(user);
        return new IdentityUserAdapter(user, roles, keepReference: true);
    }

    /// <summary>
    /// Finds a user by their unique identifier.
    /// </summary>
    /// <param name="userId">User's unique identifier</param>
    /// <returns>Identity user if found, null otherwise</returns>
    public async Task<IIdentityUser?> FindByIdAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return null;

        var roles = await userManager.GetRolesAsync(user);
        return new IdentityUserAdapter(user, roles, keepReference: true);
    }

    /// <summary>
    /// Checks if the provided password is correct for the user.
    /// </summary>
    /// <param name="user">The identity user to verify password for</param>
    /// <param name="password">Password to verify</param>
    /// <returns>True if password is correct, false otherwise</returns>
    public async Task<bool> CheckPasswordAsync(IIdentityUser user, string password)
    {
        if (user is IdentityUserAdapter adapter)
            return await userManager.CheckPasswordAsync(adapter.OriginalUser, password);

        var appUser = await userManager.FindByIdAsync(user.Id);
        return appUser is not null && await userManager.CheckPasswordAsync(appUser, password);
    }

    /// <summary>
    /// Creates a new user account.
    /// </summary>
    /// <param name="email">New user's email address</param>
    /// <param name="fullName">New user's full name</param>
    /// <param name="password">New user's password</param>
    /// <returns>True if user creation successful, false otherwise</returns>
    public async Task<bool> CreateUserAsync(string email, string fullName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            IsActive = false
        };

        var result = await userManager.CreateAsync(user, password);
        return result.Succeeded;
    }

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="roleName">Role name to assign</param>
    /// <returns>True if role assignment successful, false otherwise</returns>
    public async Task<bool> AssignRoleAsync(string email, string roleName)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return false;

        var result = await userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }

    /// <summary>
    /// Retrieves all roles assigned to a user.
    /// </summary>
    /// <param name="user">The identity user</param>
    /// <returns>List of role names assigned to user</returns>
    public async Task<IList<string>> GetUserRolesAsync(IIdentityUser user)
    {
        var appUser = await userManager.FindByIdAsync(user.Id);
        if (appUser is null)
            return [];

        return await userManager.GetRolesAsync(appUser);
    }

    /// <summary>
    /// Changes the password for a user.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="currentPassword">Current password for verification</param>
    /// <param name="newPassword">New password to set</param>
    /// <returns>True if password change successful, false otherwise</returns>
    public async Task<OperationResult> ChangePasswordAsync(string email, string currentPassword, string newPassword)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return Failed("We couldn't find your account.");

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return ToOperationResult(result);
    }

    /// <summary>
    /// Generates a password reset token for password recovery.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>Password reset token if user found, null otherwise</returns>
    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return null;

        return await userManager.GeneratePasswordResetTokenAsync(user);
    }

    /// <summary>
    /// Resets user password using a valid reset token.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="token">Password reset token</param>
    /// <param name="newPassword">New password to set</param>
    /// <returns>True if password reset successful, false otherwise</returns>
    public async Task<OperationResult> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await userManager.FindByEmailAsync(email);
        // Same generic message whether the account is missing or the token is bad,
        // so this can't be used to probe which emails are registered.
        if (user is null)
            return Failed("This password reset link is invalid or has expired.");

        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        return ToOperationResult(result);
    }

    /// <summary>Maps an Identity result to an OperationResult, carrying error descriptions.</summary>
    private static OperationResult ToOperationResult(IdentityResult result)
    {
        if (result.Succeeded)
            return new OperationResult { Success = true };

        var op = new OperationResult { Success = false };
        foreach (var error in result.Errors)
            op.AddMessage(error.Description);
        return op;
    }

    private static OperationResult Failed(string message)
    {
        var op = new OperationResult { Success = false };
        op.AddMessage(message);
        return op;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IIdentityUser>> GetAllUsersAsync()
    {
        // Two async queries regardless of user count, rather than a blocking
        // Users.ToList() followed by a GetRolesAsync round trip per user (1 + N).
        // Callers (the admin user list) only project these to DTOs, so the users
        // are read untracked.
        var users = await userManager.Users
            .AsNoTracking()
            .ToListAsync();

        // Every user/role pair in one join, then grouped in memory. Fetching all
        // pairs is equivalent to filtering by these ids, since we loaded every user.
        var roleAssignments = await (
            from userRole in identityDbContext.UserRoles
            join role in identityDbContext.Roles on userRole.RoleId equals role.Id
            select new { userRole.UserId, role.Name })
            .AsNoTracking()
            .ToListAsync();

        var rolesByUserId = roleAssignments
            .GroupBy(x => x.UserId)
            .ToDictionary(
                g => g.Key,
                g => (IList<string>)g.Select(x => x.Name!).ToList());

        return users
            .Select(user => new IdentityUserAdapter(
                user,
                rolesByUserId.TryGetValue(user.Id, out var roles) ? roles : [],
                keepReference: true))
            .ToList<IIdentityUser>();
    }

    /// <inheritdoc />
    public async Task<bool> AddToRoleByIdAsync(string userId, string roleName)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return false;

        if (await userManager.IsInRoleAsync(user, roleName))
            return true;

        var result = await userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveFromRoleByIdAsync(string userId, string roleName)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return false;

        if (!await userManager.IsInRoleAsync(user, roleName))
            return true;

        var result = await userManager.RemoveFromRoleAsync(user, roleName);
        return result.Succeeded;
    }

    /// <inheritdoc />
    public async Task<bool> SetActiveAsync(string userId, bool isActive)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return false;

        user.IsActive = isActive;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded;
    }
}