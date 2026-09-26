namespace StarterApp.API.Middleware;

/// <summary>
/// Middleware to add security headers to HTTP responses.
/// Prevents common web attacks like click-jacking, MIME sniffing, and XSS.
/// </summary>
public class SecurityHeadersMiddleware(
    RequestDelegate next,
    ILogger<SecurityHeadersMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers to response
        AddSecurityHeaders(context.Response);

        await next(context);
    }

    /// <summary>
    /// Adds comprehensive security headers to the HTTP response.
    /// </summary>
    private void AddSecurityHeaders(HttpResponse response)
    {
        // Prevent click-jacking attacks (UI redressing)
        // DENY: Page cannot be displayed in a frame at all
        // SAMEORIGIN: Page can only be displayed in a frame on the same origin as the page itself
        response.Headers["X-Frame-Options"] = "DENY";

        // Prevent MIME type sniffing
        // Tells browser not to try to guess the MIME type and always use the MIME type sent in the header
        response.Headers["X-Content-Type-Options"] = "nosniff";

        // Enable XSS protection in browsers (legacy header, but still useful)
        // 1: Enabled, mode=block: Block the page if XSS attack is detected
        response.Headers["X-XSS-Protection"] = "1; mode=block";

        // HTTP Strict-Transport-Security (HSTS)
        // Forces browser to only communicate via HTTPS
        // max-age: 31536000 seconds = 1 year
        // includeSubDomains: Apply to all subdomains
        // preload: Allow inclusion in HSTS preload list
        response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";

        // Content Security Policy (CSP)
        // Restricts sources of content that can be loaded
        // default-src 'self': All content must come from same origin by default
        // script-src 'self': Only scripts from same origin
        // style-src 'self' 'unsafe-inline': Styles from same origin and inline (required for many frameworks)
        response.Headers["Content-Security-Policy"] =
            "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self';";

        // Referrer Policy
        // Controls how much referrer information is shared when navigating to other sites
        // strict-origin-when-cross-origin: Send origin when cross-origin, full URL for same-origin
        response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Permissions Policy (formerly Feature Policy)
        // Disables certain browser features/APIs to reduce attack surface
        response.Headers["Permissions-Policy"] =
            "geolocation=(), microphone=(), camera=(), payment=(), usb=(), magnetometer=(), gyroscope=(), accelerometer=()";

        // Remove server identification header for security
        response.Headers.Remove("Server");

        // Remove X-Powered-By header to avoid exposing technology stack
        response.Headers.Remove("X-Powered-By");

        // Disable caching of sensitive content
        response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate, max-age=0";
        response.Headers["Pragma"] = "no-cache";
        response.Headers["Expires"] = "0";

        logger.LogDebug("Security headers added to response");
    }
}

/// <summary>
/// Extension member to register SecurityHeadersMiddleware in the application pipeline.
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    extension(IApplicationBuilder app)
    {
        /// <summary>
        /// Adds security headers middleware to the application.
        /// Should be called early in the middleware pipeline, before other middleware.
        /// </summary>
        /// <returns>The application builder for chaining</returns>
        public IApplicationBuilder UseSecurityHeaders()
            => app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}

