using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarterApp.Application.Common;
using StarterApp.Application.Interfaces;

namespace StarterApp.Infrastructure.Services;

/// <summary>
/// Stores uploaded files on the local file system under the configured root folder,
/// grouped into one sub-folder per reference.
/// </summary>
/// <param name="settings">File storage configuration settings.</param>
/// <param name="logger">Logger instance.</param>
public class FileStorageService(
    IOptions<FileStorageSettings> settings,
    ILogger<FileStorageService> logger) : IFileStorageService
{
    private readonly FileStorageSettings _settings = settings.Value;

    /// <inheritdoc />
    public async Task<(string StoredFileName, string FullPath, long SizeBytes)> SaveAsync(
        string reference,
        string originalFileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        // RootFolder is normally set at startup to App_Data\uploads under the content root;
        // this base-directory fallback is a safety net if that wasn't configured.
        var root = string.IsNullOrWhiteSpace(_settings.RootFolder)
            ? Path.Combine(AppContext.BaseDirectory, "App_Data", "uploads")
            : _settings.RootFolder;

        var directory = Path.Combine(root, Sanitize(reference));
        Directory.CreateDirectory(directory);

        // Prefix with a short unique token so concurrent uploads of the same file name
        // cannot overwrite each other, and never trust the client-supplied path.
        var safeName = Sanitize(originalFileName);
        var storedFileName = $"{Guid.NewGuid():N}_{safeName}";

        // Defence in depth: both components above are caller-supplied and Sanitize has
        // already stripped the directory and separator characters, but confirm the
        // resolved path is still inside the reference folder before opening it for
        // writing. Path.Combine silently discards its first argument when the second is
        // rooted, so an unsanitized component would send this write anywhere the app
        // pool identity can reach.
        var fullPath = Path.GetFullPath(Path.Combine(directory, storedFileName));
        var directoryPrefix = Path.GetFullPath(directory) + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(directoryPrefix, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogError(
                "Rejected upload: resolved path {Path} escapes the storage directory {Directory}.",
                LogSanitizer.Sanitize(fullPath), LogSanitizer.Sanitize(directory));
            throw new InvalidOperationException("Resolved upload path is outside the storage directory.");
        }

        await using var fileStream = new FileStream(
            fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await content.CopyToAsync(fileStream, cancellationToken);

        var sizeBytes = fileStream.Length;

        // Both the reference and the file name are caller-supplied, so they are
        // sanitized before logging even though Sanitize() already shaped the path.
        logger.LogInformation(
            "Stored upload for reference {Reference} at {Path} ({SizeBytes} bytes).",
            LogSanitizer.Sanitize(reference), LogSanitizer.Sanitize(fullPath), sizeBytes);

        return (storedFileName, fullPath, sizeBytes);
    }

    /// <inheritdoc />
    public void Delete(string fullPath)
    {
        try
        {
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            // Remove the reference folder once it holds nothing.
            var directory = Path.GetDirectoryName(fullPath);
            if (directory is not null &&
                Directory.Exists(directory) &&
                !Directory.EnumerateFileSystemEntries(directory).Any())
            {
                Directory.Delete(directory);
            }
        }
        catch (Exception ex)
        {
            // A leftover file is not worth failing the caller over; just log it.
            logger.LogWarning(ex, "Failed to delete stored file {Path}.", LogSanitizer.Sanitize(fullPath));
        }
    }

    /// <summary>
    /// Reduces a caller-supplied string to a single safe file-name component.
    /// Strips any directory portion, replaces characters the file system rejects
    /// (which covers the separators and the Windows drive colon), then trims the leading
    /// and trailing dots and spaces that make up traversal sequences. Never returns an
    /// empty string, so the caller always has a usable path component.
    /// </summary>
    private static string Sanitize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "_";

        // GetFileName discards "a/b/c" and "C:\dir\" style prefixes. It runs before the
        // character replacement below: replacing separators first would turn "../../x"
        // into a single component that GetFileName could no longer reduce.
        var name = Path.GetFileName(value.Trim());
        var invalid = Path.GetInvalidFileNameChars();

        // "." and ".." are legal file-name characters but meaningless as a component here.
        var cleaned = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim('.', ' ');

        return string.IsNullOrEmpty(cleaned) ? "_" : cleaned;
    }
}
