#if (useEntra)
using Microsoft.AspNetCore.Authentication;
#endif
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StarterApp.Infrastructure.Data.Contexts;
using StarterApp.IntegrationTests.Fixtures;

namespace StarterApp.IntegrationTests;

/// <summary>
/// Custom web application factory for integration testing.
/// Configures the application with in-memory database and test settings.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
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

            // Add in-memory database for testing with dedicated provider
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestAppDatabase")
                       .UseInternalServiceProvider(efServiceProvider));

#if (useAuth)
            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseInMemoryDatabase("TestIdentityDatabase")
                       .UseInternalServiceProvider(efServiceProvider));

#endif
#if (useEntra)
            // See TestApiFactory: keeps the tests off the real tenant's metadata endpoint.
            services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, _ => { });

#endif
            // Register test controllers from this assembly
            services.AddControllers()
                .AddApplicationPart(typeof(TestErrorController).Assembly);
        });
    }
}

