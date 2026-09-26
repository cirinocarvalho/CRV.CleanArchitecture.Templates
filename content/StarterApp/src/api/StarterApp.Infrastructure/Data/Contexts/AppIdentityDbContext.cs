using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StarterApp.Infrastructure.Identity;

namespace StarterApp.Infrastructure.Data.Contexts;

/// <summary>
/// Entity Framework Core database context for ASP.NET Core Identity.
/// Contains configuration for users, roles, and identity-related entities.
/// </summary>
/// <param name="options">Database context options</param>
public class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    /// <summary>
    /// Configures the database model and schema mappings for identity entities.
    /// </summary>
    /// <param name="builder">The model builder used to construct the model</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("identity");
    }
}