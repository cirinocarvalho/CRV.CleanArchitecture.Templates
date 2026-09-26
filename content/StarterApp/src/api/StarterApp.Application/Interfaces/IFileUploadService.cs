using StarterApp.Application.DTOs;

namespace StarterApp.Application.Interfaces;

/// <summary>
/// Application service interface for file upload operations.
/// Orchestrates file storage and metadata persistence.
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// Processes a file upload: stores the file and saves its metadata.
    /// </summary>
    /// <param name="request">Upload metadata.</param>
    /// <param name="fileName">Original file name.</param>
    /// <param name="fileStream">File content stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Upload response with stored file details.</returns>
    Task<FileUploadResponse> UploadFileAsync(
        FileUploadRequest request,
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all file upload detail records, newest first.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of file upload responses.</returns>
    Task<List<FileUploadResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the file upload detail records for a single reference, newest first.
    /// </summary>
    /// <param name="reference">Application-defined grouping key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of file upload responses.</returns>
    Task<List<FileUploadResponse>> GetByReferenceAsync(
        string reference,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an upload — both the stored file and its metadata row. Used to roll back an
    /// upload whose owning record failed to save, so nothing is left orphaned. No-op if the
    /// record no longer exists.
    /// </summary>
    /// <param name="id">The file upload detail id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
