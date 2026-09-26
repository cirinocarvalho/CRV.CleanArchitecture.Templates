using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using StarterApp.Application.Common;

namespace StarterApp.API.Extensions;

/// <summary>
/// Extension members for configuring logging in the application.
/// </summary>
public static class LoggingExtensions
{
    extension(WebApplicationBuilder builder)
    {
        /// <summary>
        /// Configures Serilog as the application logger, writing structured logs to the console
        /// and to rolling daily JSON files under a "logs" folder next to the deployed app.
        /// Minimum levels come from the "Serilog" section (appsettings.Logging.json); sensible
        /// code defaults are applied first so logging still works if that file is absent.
        /// </summary>
        /// <returns>The configured web application builder</returns>
        public WebApplicationBuilder AddStructuredLogging()
        {
            // The Serilog level overrides live in a dedicated file; make it available to config.
            builder.Configuration.AddJsonFile("appsettings.Logging.json", optional: true, reloadOnChange: true);

            // Logs are written next to the deployed app (e.g. the IIS site folder)\logs.
            var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logDirectory);

            builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                // Config (the "Serilog" section) overrides the code defaults above when present.
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    formatter: new CompactJsonFormatter(),
                    path: Path.Combine(logDirectory, "log-.json"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31,
                    shared: true));

            return builder;
        }
    }
}

/// <summary>
/// Custom enricher to add request context information to logs.
/// This is used by structured logging middleware to add contextual data.
/// </summary>
public class RequestContextEnricher(
    IHttpContextAccessor httpContextAccessor,
    ILogger<RequestContextEnricher> logger)
{
    /// <summary>
    /// Enriches the log context with HTTP request information.
    /// </summary>
    public void EnrichContext()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
            return;

        // Extract trace ID
        var traceId = httpContext.TraceIdentifier;

        // Extract user ID if authenticated. Claims originate in the bearer token,
        // so they are sanitized like any other request-supplied value.
        var userId = LogSanitizer.Sanitize(
            httpContext.User?.FindFirst("sub")?.Value ??
            httpContext.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value);

        // Extract username if available
        var username = LogSanitizer.Sanitize(httpContext.User?.Identity?.Name);

        // Extract client IP
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        // Extract path and method
        var path = LogSanitizer.Sanitize(httpContext.Request.Path.Value);
        var method = LogSanitizer.Sanitize(httpContext.Request.Method);

        // Log context information (structured)
        using (logger.BeginScope(new Dictionary<string, object>
        {
            { "TraceId", traceId },
            { "UserId", userId.Length == 0 ? "Anonymous" : userId },
            { "Username", username.Length == 0 ? "Anonymous" : username },
            { "ClientIp", clientIp },
            { "Path", path },
            { "Method", method }
        }))
        {
            // Context is now available to all logs within this scope
        }
    }
}
