namespace StarterApp.Application.Interfaces;

/// <summary>
/// Service interface for persisting uploaded files to the configured storage root.
/// Swap the implementation for blob/object storage without touching the application layer.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Writes a file stream to storage under the given reference. The stored file name is
    /// made unique so concurrent uploads of the same original name cannot collide.
    /// </summary>
    /// <param name="reference">Application-defined key; used as the storage sub-folder.</param>
    /// <param name="originalFileName">Original file name from the upload.</param>
    /// <param name="content">File content stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of (storedFileName, fullFilePath, sizeBytes).</returns>
    Task<(string StoredFileName, string FullPath, long SizeBytes)> SaveAsync(
        string reference,
        string originalFileName,
        Stream content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file previously written by <see cref="SaveAsync"/> (and its reference
    /// folder, if now empty). Safe to call when the file no longer exists; failures are
    /// logged rather than thrown.
    /// </summary>
    /// <param name="fullPath">Full path returned by <see cref="SaveAsync"/>.</param>
    void Delete(string fullPath);
}
