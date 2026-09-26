using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StarterApp.Infrastructure.Services;
using Xunit;

namespace StarterApp.IntegrationTests.Security;

/// <summary>
/// Guards the upload path construction in <see cref="FileStorageService"/>.
///
/// Both the reference and the original file name reach the service from the request, so
/// both are fully caller controlled. Unsanitized, a traversal sequence or a rooted path
/// in either field places the write outside the storage root - anywhere the app pool
/// identity can reach. These tests pin that behaviour shut.
/// </summary>
public class FileUploadPathTraversalTests : IDisposable
{
    private readonly string _storageRoot;
    private readonly FileStorageService _sut;

    /// <summary>
    /// Points the service at a throwaway root so the assertions can prove containment.
    /// </summary>
    public FileUploadPathTraversalTests()
    {
        _storageRoot = Path.Combine(Path.GetTempPath(), $"upload-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_storageRoot);

        _sut = new FileStorageService(
            Options.Create(new FileStorageSettings { RootFolder = _storageRoot }),
            NullLogger<FileStorageService>.Instance);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Directory.Exists(_storageRoot))
            Directory.Delete(_storageRoot, recursive: true);

        GC.SuppressFinalize(this);
    }

    private static Stream Content() => new MemoryStream(Encoding.UTF8.GetBytes("test payload"));

    [Theory]
    [InlineData(@"..\..\..\escaped")]
    [InlineData("../../../escaped")]
    [InlineData(@"..\..\..\inetpub\wwwroot\app\index")]
    public async Task SaveAsync_WithTraversalInReference_StaysInsideStorageRoot(string reference)
    {
        var (_, fullPath, _) = await _sut.SaveAsync(reference, "report.pdf", Content());

        fullPath.Should().StartWith(Path.GetFullPath(_storageRoot));
        File.Exists(fullPath).Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_WithRootedReference_DoesNotWriteToThatPath()
    {
        // Path.Combine discards its first argument when the second is rooted, so an
        // unsanitized value here would write to exactly the caller's chosen location.
        var outsideDirectory = Path.Combine(Path.GetTempPath(), $"upload-escape-{Guid.NewGuid():N}");
        Directory.CreateDirectory(outsideDirectory);

        try
        {
            var rooted = Path.Combine(outsideDirectory, "escaped");

            var (_, fullPath, _) = await _sut.SaveAsync(rooted, "report.pdf", Content());

            fullPath.Should().StartWith(Path.GetFullPath(_storageRoot));
            Directory.EnumerateFileSystemEntries(outsideDirectory).Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(outsideDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_WithTraversalInFileName_StaysInsideStorageRoot()
    {
        var (_, fullPath, _) = await _sut.SaveAsync("order-1", @"..\..\web.config", Content());

        fullPath.Should().StartWith(Path.GetFullPath(_storageRoot));
        File.Exists(fullPath).Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_WithLegitimateValues_KeepsTheOriginalFileName()
    {
        // Regression guard: sanitizing must not disturb the "<token>_<name>.<ext>"
        // stored-name convention the upload metadata records.
        var (storedFileName, fullPath, sizeBytes) = await _sut.SaveAsync(
            "order-1", "realfilename.pdf", Content());

        storedFileName.Should().EndWith("_realfilename.pdf");
        Path.GetFileName(fullPath).Should().Be(storedFileName);
        sizeBytes.Should().BeGreaterThan(0);
        File.Exists(fullPath).Should().BeTrue();
    }
}
