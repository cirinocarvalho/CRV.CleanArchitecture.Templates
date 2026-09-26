using StarterApp.Application.Interfaces;

namespace StarterApp.Infrastructure.Identity;

/// <summary>
/// Adapter pattern implementation for translating ApplicationUser to IIdentityUser interface.
/// Bridges ASP.NET Core Identity models with application layer contracts.
/// </summary>
public class IdentityUserAdapter : IIdentityUser
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; }

    /// <summary>
    /// User's full name.
    /// </summary>
    public string FullName { get; }

    /// <summary>
    /// List of roles assigned to the user.
    /// </summary>
    public IList<string> Roles { get; }

    /// <summary>
    /// Whether the account is active.
    /// </summary>
    public bool IsActive { get; }

    /// <summary>
    /// Initializes a new instance of the IdentityUserAdapter class.
    /// </summary>
    /// <param name="user">ApplicationUser to adapt</param>
    /// <param name="roles">User's assigned roles</param>
    public IdentityUserAdapter(ApplicationUser user, IList<string> roles)
    {
        Id = user.Id.ToString();
        Email = user.Email ?? string.Empty;
        FullName = user.FullName;
        Roles = roles;
        IsActive = user.IsActive;
    }

    /// <summary>
    /// Gets the original ApplicationUser if reference was kept.
    /// Internal property for accessing underlying user object.
    /// </summary>
    internal ApplicationUser OriginalUser { get; init; } = null!;

    /// <summary>
    /// Initializes a new instance with optional reference to original user.
    /// </summary>
    /// <param name="user">ApplicationUser to adapt</param>
    /// <param name="roles">User's assigned roles</param>
    /// <param name="keepReference">Whether to keep reference to original ApplicationUser</param>
    public IdentityUserAdapter(ApplicationUser user, IList<string> roles, bool keepReference)
        : this(user, roles)
    {
        if (keepReference)
            OriginalUser = user;
    }
}