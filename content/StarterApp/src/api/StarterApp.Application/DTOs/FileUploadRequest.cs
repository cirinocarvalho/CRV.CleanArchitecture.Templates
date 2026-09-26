namespace StarterApp.Application.DTOs;

/// <summary>
/// DTO for file upload requests.
/// Contains metadata required to process and store an uploaded file.
/// </summary>
public record FileUploadRequest
{
    /// <summary>
    /// Application-defined key grouping the file with the record it belongs to
    /// (for example an order number, case id, or entity id). Also used as the
    /// storage sub-folder.
    /// </summary>
    public string Reference { get; init; } = string.Empty;

    /// <summary>
    /// Name of the user performing the upload.
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// MIME content type reported by the client.
    /// </summary>
    public string ContentType { get; init; } = string.Empty;
}
