namespace StarterApp.Application.Interfaces;

/// <summary>
/// Identity user interface representing an authenticated user.
/// Provides access to user identity and role information.
/// </summary>
public interface IIdentityUser
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// User's email address.
    /// </summary>
    string Email { get; }

    /// <summary>
    /// User's full name.
    /// </summary>
    string FullName { get; }

    /// <summary>
    /// List of roles assigned to the user.
    /// </summary>
    IList<string> Roles { get; }

    /// <summary>
    /// Whether the account is active (approved by an admin).
    /// </summary>
    bool IsActive { get; }
}

