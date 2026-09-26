using StarterApp.Application.DTOs;
using StarterApp.Application.Interfaces;
using StarterApp.Application.Interfaces.Persistence;
using StarterApp.Domain.Entities;

namespace StarterApp.Application.Services;

/// <summary>
/// Application service that orchestrates file upload operations.
/// Coordinates file storage and database persistence.
/// </summary>
/// <param name="fileStorage">File storage service for writing files to storage.</param>
/// <param name="repository">Repository for FileUploadDetail persistence.</param>
/// <param name="unitOfWork">Unit of work for transaction management.</param>
public class FileUploadService(
    IFileStorageService fileStorage,
    IRepositoryBase<FileUploadDetail> repository,
    IUnitOfWork unitOfWork) : IFileUploadService
{

    /// <inheritdoc />
    public async Task<FileUploadResponse> UploadFileAsync(
        FileUploadRequest request,
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        // 1. Write the file to storage first — it is the expensive, failure-prone step,
        // and there is no metadata row to clean up if it throws.
        var (storedFileName, fullPath, sizeBytes) = await fileStorage.SaveAsync(
            request.Reference, fileName, fileStream, cancellationToken);

        var detail = new FileUploadDetail
        {
            FileName = storedFileName,
            OriginalFileName = fileName,
            FilePath = fullPath,
            ContentType = request.ContentType,
            SizeBytes = sizeBytes,
            UserName = request.UserName,
            Reference = request.Reference,
            CreatedDate = DateTime.UtcNow
        };

        // 2. Persist the metadata. If the save fails, drop the file we just wrote so it
        // isn't left on disk with nothing pointing at it.
        try
        {
            await repository.AddAsync(detail);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            fileStorage.Delete(fullPath);
            throw;
        }

        return ToResponse(detail);
    }

    /// <inheritdoc />
    public async Task<List<FileUploadResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var details = await repository.GetAllAsync(cancellationToken);

        return details
            .OrderByDescending(d => d.CreatedDate)
            .Select(ToResponse)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<List<FileUploadResponse>> GetByReferenceAsync(
        string reference,
        CancellationToken cancellationToken = default)
    {
        var details = await repository.GetByConditionAsync(
            d => d.Reference == reference, cancellationToken);

        return details
            .OrderByDescending(d => d.CreatedDate)
            .Select(ToResponse)
            .ToList();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var detail = await repository.GetByIdAsync(id, cancellationToken);
        if (detail is null)
            return;

        await repository.DeleteAsync(detail);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Remove the file only after the row is gone, so a failed delete can be retried
        // rather than leaving a row pointing at a file that no longer exists.
        fileStorage.Delete(detail.FilePath);
    }

    private static FileUploadResponse ToResponse(FileUploadDetail detail) => new()
    {
        Id = detail.Id,
        FileName = detail.FileName,
        OriginalFileName = detail.OriginalFileName,
        FilePath = detail.FilePath,
        Reference = detail.Reference,
        SizeBytes = detail.SizeBytes,
        CreatedDate = detail.CreatedDate
    };
}
