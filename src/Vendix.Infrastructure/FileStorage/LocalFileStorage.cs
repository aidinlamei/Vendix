using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vendix.Application.Common.Interfaces;

namespace Vendix.Infrastructure.FileStorage;

/// <summary>
/// Local file system implementation of IFileStorage
/// </summary>
public class LocalFileStorage : IFileStorage
{
    private readonly FileStorageSettings _settings;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(
        IOptions<FileStorageSettings> settings,
        ILogger<LocalFileStorage> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> UploadAsync(
        Stream file,
        string fileName,
        string folder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(folder);

        // Validate file size
        if (file.Length > _settings.MaxFileSizeBytes)
        {
            throw new InvalidOperationException(
                $"File size ({file.Length} bytes) exceeds maximum allowed ({_settings.MaxFileSizeBytes} bytes)");
        }

        // Validate extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_settings.AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                $"File extension '{extension}' is not allowed. Allowed: {string.Join(", ", _settings.AllowedExtensions)}");
        }

        // Generate unique filename: {guid}_{sanitized_original_name}{extension}
        var sanitizedName = SanitizeFileName(Path.GetFileNameWithoutExtension(fileName));
        var uniqueFileName = $"{Guid.NewGuid():N}_{sanitizedName}{extension}";
        
        // Build full path
        var folderPath = Path.Combine(_settings.BasePath, folder);
        var fullPath = Path.Combine(folderPath, uniqueFileName);

        // Ensure directory exists
        Directory.CreateDirectory(folderPath);

        // Save file
        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await file.CopyToAsync(fileStream, cancellationToken);

        // Return relative path (for storage in DB)
        var relativePath = Path.Combine(folder, uniqueFileName).Replace("\\", "/");
        
        _logger.LogInformation("File uploaded: {Path} ({Size} bytes)", relativePath, file.Length);
        
        return relativePath;
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Task.CompletedTask;

        var fullPath = Path.Combine(_settings.BasePath, path);
        
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("File deleted: {Path}", path);
        }
        else
        {
            _logger.LogWarning("File not found for deletion: {Path}", path);
        }

        return Task.CompletedTask;
    }

    public string GetPublicUrl(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;
            
        // Ensure forward slashes for URL
        var normalizedPath = path.Replace("\\", "/");
        return $"{_settings.BaseUrl}/{normalizedPath}";
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Task.FromResult(false);
            
        var fullPath = Path.Combine(_settings.BasePath, path);
        return Task.FromResult(File.Exists(fullPath));
    }

    /// <summary>
    /// Remove invalid characters from filename
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        
        // Limit length and remove spaces
        sanitized = sanitized.Replace(" ", "-").ToLowerInvariant();
        
        if (sanitized.Length > 50)
            sanitized = sanitized[..50];
            
        return string.IsNullOrWhiteSpace(sanitized) ? "file" : sanitized;
    }
}

