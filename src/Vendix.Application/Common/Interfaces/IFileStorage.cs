namespace Vendix.Application.Common.Interfaces;

/// <summary>
/// File storage abstraction for uploading and managing files
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Upload a file to storage
    /// </summary>
    /// <param name="file">File stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="folder">Target folder (e.g., "products", "brands")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Relative path to the uploaded file</returns>
    Task<string> UploadAsync(Stream file, string fileName, string folder, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete a file from storage
    /// </summary>
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get public URL for a file
    /// </summary>
    string GetPublicUrl(string path);
    
    /// <summary>
    /// Check if file exists
    /// </summary>
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);
}

