using System.Net;
using StarterApp.Application.Common;

namespace StarterApp.API.Middleware;

/// <summary>
/// Security logging middleware that tracks and logs security-related events.
/// Logs authentication attempts, authorization failures, suspicious patterns, and security events.
/// Every request-derived value (path, method, forwarded IP) passes through
/// <see cref="LogSanitizer"/> before it reaches a log call or scope, so a crafted
/// request cannot inject fake log lines.
/// </summary>
public class SecurityLoggingMiddleware(
    RequestDelegate next,
    ILogger<SecurityLoggingMiddleware> logger)
{
    private static readonly string[] AuthPaths =
    [
        "/auth/login",
        "/auth/register",
        "/auth/forgot-password",
        "/auth/reset-password",
        "/auth/change-password",
        "/api/v1/auth/login",
        "/api/v1/auth/register",
        "/api/v1/auth/forgot-password",
        "/api/v1/auth/reset-password",
        "/api/v1/auth/change-password"
    ];

    private static readonly string[] SensitivePaths =
    [
        "/auth",
        "/profile",
        "/roles",
        "/users",
        "/admin",
        "/settings"
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var remoteIpAddress = GetClientIpAddress(context);

        // Log incoming request
        LogIncomingRequest(context, remoteIpAddress);

        // Call next middleware
        await next(context);

        var duration = DateTime.UtcNow - startTime;

        // Log response and check for security events
        LogResponseAndSecurityEvents(context, remoteIpAddress, duration);
    }

    /// <summary>
    /// Gets the client's IP address, handling proxy scenarios.
    /// The proxy headers are client-controlled, so the result is sanitized for logging.
    /// </summary>
    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP (from proxy)
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ips = forwardedFor.ToString().Split(',');
            return LogSanitizer.Sanitize(ips[0].Trim());
        }

        // Check for CloudFlare IP
        if (context.Request.Headers.TryGetValue("CF-Connecting-IP", out var cfIp))
        {
            return LogSanitizer.Sanitize(cfIp.ToString());
        }

        // Fall back to remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    /// <summary>
    /// Logs information about the incoming request.
    /// </summary>
    private void LogIncomingRequest(HttpContext context, string remoteIpAddress)
    {
        var path = LogSanitizer.Sanitize(context.Request.Path.Value);
        var method = LogSanitizer.Sanitize(context.Request.Method);

        // Log authentication attempts
        if (IsAuthenticationEndpoint(path))
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                { "EventType", "AuthenticationAttempt" },
                { "RemoteIpAddress", remoteIpAddress },
                { "Path", path },
                { "Method", method },
                { "Timestamp", DateTime.UtcNow }
            }))
            {
                logger.LogInformation("Authentication attempt from {RemoteIpAddress} to {Path}",
                    remoteIpAddress, path);
            }
        }

        // Log API calls to sensitive endpoints
        if (IsSensitiveEndpoint(path))
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                { "EventType", "SensitiveEndpointAccess" },
                { "RemoteIpAddress", remoteIpAddress },
                { "Path", path },
                { "Method", method },
                { "Timestamp", DateTime.UtcNow }
            }))
            {
                logger.LogInformation("Sensitive endpoint accessed: {Method} {Path} from {RemoteIpAddress}",
                    method, path, remoteIpAddress);
            }
        }
    }

    /// <summary>
    /// Logs response status and security-related events.
    /// </summary>
    private void LogResponseAndSecurityEvents(HttpContext context, string remoteIpAddress, TimeSpan duration)
    {
        var statusCode = context.Response.StatusCode;
        var path = LogSanitizer.Sanitize(context.Request.Path.Value);
        var method = LogSanitizer.Sanitize(context.Request.Method);

        // Log failed authentication attempts
        if (statusCode == (int)HttpStatusCode.Unauthorized)
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                { "EventType", "UnauthorizedAttempt" },
                { "RemoteIpAddress", remoteIpAddress },
                { "Path", path },
                { "Method", method },
                { "StatusCode", statusCode },
                { "Timestamp", DateTime.UtcNow }
            }))
            {
                logger.LogWarning(
                    "SECURITY: Unauthorized access attempt from {RemoteIpAddress} to {Method} {Path}",
                    remoteIpAddress, method, path);
            }
        }

        // Log forbidden access attempts
        if (statusCode == (int)HttpStatusCode.Forbidden)
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                { "EventType", "ForbiddenAccess" },
                { "RemoteIpAddress", remoteIpAddress },
                { "Path", path },
                { "Method", method },
                { "StatusCode", statusCode },
                { "Timestamp", DateTime.UtcNow }
            }))
            {
                logger.LogWarning(
                    "SECURITY: Forbidden access from {RemoteIpAddress} to {Method} {Path}",
                    remoteIpAddress, method, path);
            }
        }

        // Log server errors
        if (statusCode >= (int)HttpStatusCode.InternalServerError)
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                { "EventType", "ServerError" },
                { "RemoteIpAddress", remoteIpAddress },
                { "Path", path },
                { "Method", method },
                { "StatusCode", statusCode },
                { "Duration", duration.TotalMilliseconds },
                { "Timestamp", DateTime.UtcNow }
            }))
            {
                logger.LogError(
                    "Server error: {StatusCode} from {RemoteIpAddress} to {Method} {Path} - Duration: {Duration}ms",
                    statusCode, remoteIpAddress, method, path, duration.TotalMilliseconds);
            }
        }

        // Log successful sensitive operations
        if (statusCode < (int)HttpStatusCode.BadRequest && IsSensitiveEndpoint(path))
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                { "EventType", "SensitiveOperationSuccess" },
                { "RemoteIpAddress", remoteIpAddress },
                { "Path", path },
                { "Method", method },
                { "StatusCode", statusCode },
                { "Duration", duration.TotalMilliseconds },
                { "Timestamp", DateTime.UtcNow }
            }))
            {
                logger.LogInformation(
                    "Sensitive operation completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms",
                    method, path, statusCode, duration.TotalMilliseconds);
            }
        }

        // Log slow requests (potential issues)
        if (duration.TotalMilliseconds > 5000)
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                { "EventType", "SlowRequest" },
                { "RemoteIpAddress", remoteIpAddress },
                { "Path", path },
                { "Method", method },
                { "StatusCode", statusCode },
                { "Duration", duration.TotalMilliseconds },
                { "Timestamp", DateTime.UtcNow }
            }))
            {
                logger.LogWarning(
                    "Slow request detected: {Method} {Path} took {Duration}ms",
                    method, path, duration.TotalMilliseconds);
            }
        }
    }

    /// <summary>
    /// Determines if the endpoint is an authentication endpoint.
    /// </summary>
    private static bool IsAuthenticationEndpoint(string path)
        => AuthPaths.Any(p => path.Contains(p, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Determines if the endpoint accesses sensitive operations.
    /// </summary>
    private static bool IsSensitiveEndpoint(string path)
        => SensitivePaths.Any(p => path.Contains(p, StringComparison.OrdinalIgnoreCase));
}

/// <summary>
/// Extension member to register security logging middleware.
/// </summary>
public static class SecurityLoggingMiddlewareExtensions
{
    extension(IApplicationBuilder app)
    {
        /// <summary>
        /// Adds security logging middleware to the application.
        /// Should be registered early in the middleware pipeline.
        /// </summary>
        /// <returns>The application builder for chaining</returns>
        public IApplicationBuilder UseSecurityLogging()
            => app.UseMiddleware<SecurityLoggingMiddleware>();
    }
}

