using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using StarterApp.Application.Common;

namespace StarterApp.API.Extensions;

/// <summary>
/// Rate limiting for the anonymous authentication endpoints. Without this,
/// login / register / forgot-password / reset-password accept unlimited attempts,
/// which leaves credential stuffing and reset-token guessing unbounded.
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>Policy for login, register and reset-password.</summary>
    public const string AuthPolicy = "auth";

    /// <summary>Policy for forgot-password, which can send email per request.</summary>
    public const string AuthEmailPolicy = "auth-email";

    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the rate limiter and its two authentication policies.
        /// Only endpoints carrying [EnableRateLimiting] are affected; there is no
        /// global limiter, so the rest of the API is untouched.
        /// </summary>
        public IServiceCollection AddAuthRateLimiting(IConfiguration configuration)
        {
            var settings = configuration
                .GetSection(RateLimitSettings.SectionName)
                .Get<RateLimitSettings>() ?? new RateLimitSettings();

            services.AddRateLimiter(options =>
            {
                // The framework default is 503 Service Unavailable, which is
                // misleading. 429 is what clients and scanners expect.
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = (context, _) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter =
                            ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                    }

                    context.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger(typeof(RateLimitingExtensions).FullName!)
                        .LogWarning(
                            "Rate limit rejected {Method} {Path} from {RemoteIp}",
                            LogSanitizer.Sanitize(context.HttpContext.Request.Method),
                            LogSanitizer.Sanitize(context.HttpContext.Request.Path.Value),
                            context.HttpContext.Connection.RemoteIpAddress);

                    return ValueTask.CompletedTask;
                };

                options.AddPolicy(AuthPolicy, httpContext =>
                    PerClientFixedWindow(httpContext, settings.AuthPermitLimit, settings.WindowSeconds));

                options.AddPolicy(AuthEmailPolicy, httpContext =>
                    PerClientFixedWindow(httpContext, settings.EmailPermitLimit, settings.WindowSeconds));
            });

            return services;
        }
    }

    /// <summary>
    /// Builds a fixed-window partition keyed on the caller's IP address.
    /// </summary>
    /// <remarks>
    /// Two caveats worth knowing before tuning the limits:
    /// users behind a shared NAT egress all land in the same partition, so limits
    /// must stay generous enough for a whole office; and if this app is ever put
    /// behind a reverse proxy, RemoteIpAddress becomes the proxy's address unless
    /// forwarded headers are configured, collapsing everyone into one partition.
    /// Partitioning on the submitted email instead would be tighter, but would let
    /// an attacker lock a specific user out on demand.
    /// </remarks>
    private static RateLimitPartition<string> PerClientFixedWindow(
        HttpContext httpContext,
        int permitLimit,
        int windowSeconds) =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                // Reject immediately rather than holding connections open.
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
}
