using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StarterApp.Infrastructure.Data.Contexts;

namespace StarterApp.API.Extensions;

/// <summary>
/// Health endpoints for load balancers, IIS Application Initialization and uptime monitors.
/// Two endpoints, because the questions differ: "is the process up?" (liveness) must not
/// depend on SQL, or a database blip would have the host recycle a perfectly good app;
/// "can it actually serve traffic?" (readiness) must.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>Tag for checks that gate readiness (dependencies, not the process itself).</summary>
    private const string ReadyTag = "ready";

    /// <summary>Liveness endpoint - the process answers, nothing else is asserted.</summary>
    public const string LiveEndpoint = "/health";

    /// <summary>Readiness endpoint - runs the dependency checks.</summary>
    public const string ReadyEndpoint = "/health/ready";

    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers connectivity checks for the database contexts. Each is capped at a
        /// short timeout so a hung SQL connect surfaces as Unhealthy rather than leaving the
        /// monitor waiting on the ADO connect timeout.
        /// </summary>
        public IServiceCollection AddAppHealthChecks()
        {
            services.AddHealthChecks()
#if (useAuth)
                .AddCheck<DbConnectivityHealthCheck<AppIdentityDbContext>>(
                    name: "identity-db",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: [ReadyTag],
                    timeout: TimeSpan.FromSeconds(5))
#endif
                .AddCheck<DbConnectivityHealthCheck<AppDbContext>>(
                    name: "app-db",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: [ReadyTag],
                    timeout: TimeSpan.FromSeconds(5));

            return services;
        }
    }

    extension(WebApplication app)
    {
        /// <summary>
        /// Maps the liveness and readiness endpoints. Both are anonymous - a monitor that
        /// needs a JWT is a monitor that reports outages when auth is misconfigured - so the
        /// readiness payload deliberately carries status names only, never exception text or
        /// connection details.
        /// </summary>
        public WebApplication MapAppHealthChecks()
        {
            app.MapHealthChecks(LiveEndpoint, new()
            {
                // Run no checks: reaching this line already proves the process is alive.
                Predicate = _ => false
            }).AllowAnonymous();

            app.MapHealthChecks(ReadyEndpoint, new()
            {
                Predicate = registration => registration.Tags.Contains(ReadyTag),
                ResponseWriter = WriteReadyResponse
            }).AllowAnonymous();

            return app;
        }
    }

    /// <summary>
    /// Writes { "status": ..., "checks": { name: status } } - enough to tell which
    /// dependency is down without disclosing why to an unauthenticated caller.
    /// </summary>
    private static async Task WriteReadyResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        using var stream = new MemoryStream();
        await using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("status", report.Status.ToString());

            writer.WriteStartObject("checks");
            foreach (var entry in report.Entries)
                writer.WriteString(entry.Key, entry.Value.Status.ToString());
            writer.WriteEndObject();

            writer.WriteEndObject();
        }

        await context.Response.Body.WriteAsync(stream.ToArray());
    }
}

/// <summary>
/// Verifies the application can open a connection to the database behind
/// <typeparamref name="TContext"/>. Uses CanConnectAsync rather than a query so the check
/// stays cheap and does not depend on any particular table existing.
/// </summary>
/// <param name="dbContext">The database context to probe.</param>
public sealed class DbConnectivityHealthCheck<TContext>(TContext dbContext) : IHealthCheck
    where TContext : DbContext
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                // The description is logged and shown to diagnostics consumers, not
                // written to the anonymous response body.
                : HealthCheckResult.Unhealthy($"{typeof(TContext).Name}: cannot connect.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"{typeof(TContext).Name}: connection failed.", ex);
        }
    }
}
