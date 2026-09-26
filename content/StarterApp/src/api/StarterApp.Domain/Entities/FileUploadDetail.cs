namespace StarterApp.Domain.Entities;

/// <summary>
/// Entity representing metadata for an uploaded file.
/// Stores file information, ownership, and the reference it belongs to.
/// </summary>
public class FileUploadDetail
{
    /// <summary>
    /// Unique identifier for the file upload record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name the file is stored under on disk (unique per upload).
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Original file name as supplied by the client.
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// Full path where the file is stored on disk.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// MIME content type reported by the client.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Size of the stored file in bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Name of the user who uploaded the file.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Application-defined key grouping the file with the record it belongs to
    /// (for example an order number, case id, or entity id).
    /// </summary>
    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the file was uploaded (UTC).
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
