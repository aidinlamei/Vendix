using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Vendix.Infrastructure.FileStorage;

namespace Vendix.Infrastructure.Tests.FileStorage;

public class LocalFileStorageTests : IDisposable
{
    private readonly string _testBasePath;
    private readonly LocalFileStorage _sut;
    private readonly FileStorageSettings _settings;

    public LocalFileStorageTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), $"vendix_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testBasePath);

        _settings = new FileStorageSettings
        {
            BasePath = _testBasePath,
            BaseUrl = "/uploads",
            MaxFileSizeBytes = 1024 * 1024, // 1MB for tests
            AllowedExtensions = [".jpg", ".png", ".webp"]
        };

        var options = Options.Create(_settings);
        var logger = Substitute.For<ILogger<LocalFileStorage>>();

        _sut = new LocalFileStorage(options, logger, Substitute.For<IWebHostEnvironment>());
    }

    public void Dispose()
    {
        if (Directory.Exists(_testBasePath))
            Directory.Delete(_testBasePath, recursive: true);
    }

    [Fact]
    public async Task UploadAsync_ValidFile_SavesAndReturnsPath()
    {
        // Arrange
        var content = "test file content"u8.ToArray();
        using var stream = new MemoryStream(content);

        // Act
        var path = await _sut.UploadAsync(stream, "test-image.jpg", "products");

        // Assert
        path.Should().StartWith("products/");
        path.Should().EndWith(".jpg");
        path.Should().Contain("test-image");

        var fullPath = Path.Combine(_testBasePath, path);
        File.Exists(fullPath).Should().BeTrue();
    }

    [Fact]
    public async Task UploadAsync_InvalidExtension_ThrowsException()
    {
        // Arrange
        using var stream = new MemoryStream([1, 2, 3]);

        // Act
        var act = () => _sut.UploadAsync(stream, "malware.exe", "products");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*extension*not allowed*");
    }

    [Fact]
    public async Task UploadAsync_FileTooLarge_ThrowsException()
    {
        // Arrange
        var largeContent = new byte[2 * 1024 * 1024]; // 2MB > 1MB limit
        using var stream = new MemoryStream(largeContent);

        // Act
        var act = () => _sut.UploadAsync(stream, "large.jpg", "products");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*exceeds maximum*");
    }

    [Fact]
    public async Task DeleteAsync_ExistingFile_DeletesFile()
    {
        // Arrange
        using var stream = new MemoryStream([1, 2, 3]);
        var path = await _sut.UploadAsync(stream, "to-delete.jpg", "products");
        var fullPath = Path.Combine(_testBasePath, path);
        File.Exists(fullPath).Should().BeTrue();

        // Act
        await _sut.DeleteAsync(path);

        // Assert
        File.Exists(fullPath).Should().BeFalse();
    }

    [Fact]
    public void GetPublicUrl_ValidPath_ReturnsCorrectUrl()
    {
        // Arrange
        var path = "products/abc123_image.jpg";

        // Act
        var url = _sut.GetPublicUrl(path);

        // Assert
        url.Should().Be("/uploads/products/abc123_image.jpg");
    }

    [Fact]
    public async Task ExistsAsync_ExistingFile_ReturnsTrue()
    {
        // Arrange
        using var stream = new MemoryStream([1, 2, 3]);
        var path = await _sut.UploadAsync(stream, "exists.jpg", "products");

        // Act
        var exists = await _sut.ExistsAsync(path);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingFile_ReturnsFalse()
    {
        // Act
        var exists = await _sut.ExistsAsync("products/does-not-exist.jpg");

        // Assert
        exists.Should().BeFalse();
    }
}
