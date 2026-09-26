namespace StarterApp.Infrastructure.Services;

/// <summary>
/// Configuration settings for local file storage.
/// Bound from the "FileStorage" configuration section.
/// </summary>
public class FileStorageSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Root folder uploaded files are written to. When not set, defaults to
    /// "App_Data\uploads" under the app content root (wired up at startup in Program.cs).
    /// </summary>
    public string RootFolder { get; set; } = string.Empty;

    /// <summary>
    /// Maximum accepted upload size in bytes. Defaults to 50 MB.
    /// </summary>
    public long MaxBytes { get; set; } = 50 * 1024 * 1024;
}
