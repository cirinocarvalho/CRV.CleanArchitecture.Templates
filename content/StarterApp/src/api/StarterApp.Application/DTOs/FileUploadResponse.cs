namespace StarterApp.Application.DTOs;

/// <summary>
/// DTO returned after a successful file upload.
/// Contains details about the stored file.
/// </summary>
public record FileUploadResponse
{
    /// <summary>
    /// Unique identifier of the file upload record.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Name the file is stored under on disk.
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Original file name as supplied by the client.
    /// </summary>
    public string OriginalFileName { get; init; } = string.Empty;

    /// <summary>
    /// Full path where the file was stored.
    /// </summary>
    public string FilePath { get; init; } = string.Empty;

    /// <summary>
    /// Application-defined key grouping the file with the record it belongs to.
    /// </summary>
    public string Reference { get; init; } = string.Empty;

    /// <summary>
    /// Size of the stored file in bytes.
    /// </summary>
    public long SizeBytes { get; init; }

    /// <summary>
    /// Date and time when the file was uploaded (UTC).
    /// </summary>
    public DateTime CreatedDate { get; init; }
}
