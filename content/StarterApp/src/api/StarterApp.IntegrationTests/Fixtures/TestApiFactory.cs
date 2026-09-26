#if (useEntra)
using Microsoft.AspNetCore.Authentication;
#endif
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StarterApp.Infrastructure.Data.Contexts;

namespace StarterApp.IntegrationTests.Fixtures;

/// <summary>
/// Web application factory for integration testing.
/// Configures the test environment with in-memory databases to isolate tests from external dependencies.
/// </summary>
public class TestApiFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Configures the web host with in-memory database contexts for testing.
    /// Replaces production database contexts with in-memory alternatives.
    /// </summary>
    /// <param name="builder">The web host builder to configure</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing database context registrations
            services.RemoveAll<DbContextOptions<AppDbContext>>();
#if (useAuth)
            services.RemoveAll<DbContextOptions<AppIdentityDbContext>>();
#endif

            // Build a dedicated EF Core service provider with only InMemory to avoid
            // conflict with Npgsql services registered by production code
            var efServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

#if (useAuth)
            // Add InMemory replacements with dedicated provider
            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseInMemoryDatabase("TestIdentityDb")
                       .UseInternalServiceProvider(efServiceProvider));

#endif
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestAppDb")
                       .UseInternalServiceProvider(efServiceProvider));
#if (useEntra)

            // Replaces Entra as the default scheme. Registered last, so this wins over the
            // bearer handler the app configured — which would otherwise try to reach the
            // tenant's metadata endpoint on the first authenticated request.
            //
            // Setting DefaultScheme alone is not enough; see the matching note in
            // Infrastructure/DependencyInjection.cs.
            services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, _ => { });
#endif
        });

        builder.UseEnvironment("Development");
    }
}