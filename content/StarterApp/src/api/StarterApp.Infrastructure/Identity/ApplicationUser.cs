using Microsoft.AspNetCore.Identity;

namespace StarterApp.Infrastructure.Identity;

/// <summary>
/// Application user extending ASP.NET Core Identity with custom properties.
/// Represents a user in the system with identity and authentication information.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// User's full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the account is active. New registrations start inactive until an admin approves them.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
