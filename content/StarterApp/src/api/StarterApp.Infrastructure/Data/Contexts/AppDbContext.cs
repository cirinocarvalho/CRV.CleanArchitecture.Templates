using Microsoft.EntityFrameworkCore;
using StarterApp.Domain.Entities;

namespace StarterApp.Infrastructure.Data.Contexts;

/// <summary>
/// Entity Framework Core database context for application data.
/// Contains database sets for domain entities and configurations.
/// </summary>
/// <param name="options">Database context options</param>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Name of the case-insensitive ICU collation defined in <see cref="OnModelCreating"/>.
    /// </summary>
    public const string CaseInsensitiveCollation = "case_insensitive";

    /// <summary>
    /// Gets the FileUploadDetails database set.
    /// </summary>
    public DbSet<FileUploadDetail> FileUploadDetails => Set<FileUploadDetail>();

#if (useAuth)
    /// <summary>
    /// Gets the Companies database set.
    /// </summary>
    public DbSet<Company> Companies => Set<Company>();

    /// <summary>
    /// Gets the UserCompanies database set (user-to-company assignment).
    /// </summary>
    public DbSet<UserCompany> UserCompanies => Set<UserCompany>();

#endif
#if (includeSamples)
    /// <summary>
    /// Gets the Products database set. Sample entity — delete it along with the rest of
    /// the Product slice once you have your own.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

#endif
    /// <summary>
    /// Configures the database model and schema mappings.
    /// </summary>
    /// <param name="builder">The model builder used to construct the model</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("public");

        // PostgreSQL compares text case-sensitively, unlike SQL Server's default collation.
        // Natural keys that people type — company names, SKUs — use this ICU collation so
        // "Acme" and "acme" are the same value, for lookups and for their unique indexes.
        builder.HasCollation(CaseInsensitiveCollation, locale: "und-u-ks-level2", provider: "icu", deterministic: false);
#if (useAuth)

        builder.Entity<Company>(e =>
        {
            e.Property(c => c.Name).UseCollation(CaseInsensitiveCollation);
            e.HasIndex(c => c.Name).IsUnique();
        });

        builder.Entity<UserCompany>(e =>
        {
            e.HasKey(uc => uc.UserId);
            e.HasOne(uc => uc.Company)
             .WithMany()
             .HasForeignKey(uc => uc.CompanyId);
        });
#endif

        builder.Entity<FileUploadDetail>(e =>
        {
            e.HasIndex(f => f.Reference);
        });
#if (includeSamples)

        builder.Entity<Product>(e =>
        {
            e.Property(p => p.Price).HasPrecision(18, 2);
            e.Property(p => p.Sku).UseCollation(CaseInsensitiveCollation);
            e.HasIndex(p => p.Sku).IsUnique();
        });
#endif
    }
}
