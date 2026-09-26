namespace StarterApp.Infrastructure.Identity;

/// <summary>
/// JWT configuration settings for token generation and validation.
/// Configures JWT bearer token parameters and expiration.
/// </summary>
public class JwtSettings
{
    /// <summary>Configuration section name in appsettings.json</summary>
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// Secret key used for signing JWT tokens.
    /// Must be at least 32 characters for HS256.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Issuer claim identifies the principal that issued the JWT.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Audience claim identifies the recipients that the JWT is intended for.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in minutes.
    /// Default is 60 minutes.
    /// </summary>
    public int ExpirationInMinutes { get; set; } = 60;
}