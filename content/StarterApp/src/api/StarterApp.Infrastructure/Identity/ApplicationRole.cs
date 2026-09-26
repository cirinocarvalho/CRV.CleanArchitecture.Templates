using Microsoft.AspNetCore.Identity;

namespace StarterApp.Infrastructure.Identity;

/// <summary>
/// Application role extending ASP.NET Core Identity.
/// Represents a role in the system for role-based access control (RBAC).
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
}
