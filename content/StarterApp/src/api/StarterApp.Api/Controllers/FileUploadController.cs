using Asp.Versioning;
#if (useAuth)
using Microsoft.AspNetCore.Authorization;
#endif
using Microsoft.AspNetCore.Mvc;
using StarterApp.Application.DTOs;
using StarterApp.Application.Interfaces;

namespace StarterApp.API.Controllers;

/// <summary>
/// Controller for file upload operations.
/// Handles uploading files and reading back their stored metadata.
/// </summary>
/// <param name="fileUploadService">File upload service for processing uploads.</param>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
#if (useAuth)
[Authorize]
#endif
public class FileUploadController(IFileUploadService fileUploadService) : ControllerBase
{
    /// <summary>
    /// Uploads a file and associates it with an application-defined reference.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="reference">Key grouping the file with the record it belongs to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// 200 OK with upload response details.
    /// 400 Bad Request if no file is provided.
    /// </returns>
    [HttpPost]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] string reference,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { Message = "No file provided." });

        var request = new FileUploadRequest
        {
            Reference = reference,
            ContentType = file.ContentType,
            UserName = User.Identity?.Name ?? string.Empty
        };

        await using var stream = file.OpenReadStream();
        var response = await fileUploadService.UploadFileAsync(
            request, file.FileName, stream, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returns file upload records, optionally filtered to a single reference.
    /// </summary>
    /// <param name="reference">Optional reference to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 OK with list of file upload details.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? reference,
        CancellationToken cancellationToken)
    {
        var results = string.IsNullOrWhiteSpace(reference)
            ? await fileUploadService.GetAllAsync(cancellationToken)
            : await fileUploadService.GetByReferenceAsync(reference, cancellationToken);

        return Ok(results);
    }

    /// <summary>
    /// Deletes an upload and its stored file.
    /// </summary>
    /// <param name="id">The file upload detail id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await fileUploadService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
