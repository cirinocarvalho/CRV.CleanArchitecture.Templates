namespace StarterApp.API.Extensions;

/// <summary>
/// Configuration settings for rate limiting the anonymous authentication
/// endpoints. Limits are applied per client IP address over a fixed window.
/// All values have working defaults, so the configuration section is optional.
/// </summary>
public class RateLimitSettings
{
    /// <summary>Configuration section name in appsettings.json</summary>
    public const string SectionName = "RateLimitSettings";

    /// <summary>Length of the fixed window, in seconds, for both policies.</summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Requests allowed per window, per IP, for login / register / reset-password.
    /// Generous enough for an office sharing one NAT egress address, while still
    /// far below what credential stuffing or token guessing requires.
    /// </summary>
    public int AuthPermitLimit { get; set; } = 20;

    /// <summary>
    /// Requests allowed per window, per IP, for forgot-password. Lower than
    /// <see cref="AuthPermitLimit"/> because each accepted request can send an
    /// email, so abuse here floods a real person's inbox.
    /// </summary>
    public int EmailPermitLimit { get; set; } = 5;
}
