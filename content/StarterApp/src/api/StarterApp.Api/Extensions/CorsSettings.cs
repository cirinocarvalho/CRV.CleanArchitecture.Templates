namespace StarterApp.API.Extensions;

/// <summary>
/// Configuration settings for Cross-Origin Resource Sharing (CORS).
/// Specifies which origins are allowed to make cross-origin requests to the API.
/// </summary>
public class CorsSettings
{
    /// <summary>Configuration section name in appsettings.json</summary>
    public const string SectionName = "CorsSettings";

    /// <summary>
    /// Array of allowed origins that can make cross-origin requests.
    /// Example: ["https://localhost:3000", "https://example.com"]
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];
}