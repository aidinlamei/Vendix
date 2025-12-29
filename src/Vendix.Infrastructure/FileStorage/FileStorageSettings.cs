namespace Vendix.Infrastructure.FileStorage;

/// <summary>
/// Settings for file storage configuration
/// </summary>
public class FileStorageSettings
{
    public const string SectionName = "FileStorage";
    
    /// <summary>
    /// Base path for file storage (default: wwwroot/uploads)
    /// </summary>
    public string BasePath { get; set; } = "wwwroot/uploads";
    
    /// <summary>
    /// Base URL for public access (default: /uploads)
    /// </summary>
    public string BaseUrl { get; set; } = "/uploads";
    
    /// <summary>
    /// Maximum file size in bytes (default: 5MB)
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    
    /// <summary>
    /// Allowed file extensions
    /// </summary>
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    
    /// <summary>
    /// Maximum image dimension (width or height) for resizing
    /// </summary>
    public int MaxImageDimension { get; set; } = 1200;
}

