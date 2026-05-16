namespace AlgoSphere.Application.Interfaces;

/// <summary>
/// Abstraction over object storage (MinIO in dev, AWS S3 in prod).
/// Switch between them by changing appsettings — zero code changes needed.
/// </summary>
public interface IStorageService
{
    /// <summary>Generate a pre-signed URL that allows direct browser upload (PUT).</summary>
    Task<string> GetPresignedUploadUrlAsync(string objectKey, int expirySeconds = 3600);

    /// <summary>Generate a pre-signed URL that allows direct browser download (GET).</summary>
    Task<string> GetPresignedDownloadUrlAsync(string objectKey, int expirySeconds = 3600);

    /// <summary>Delete an object from the bucket.</summary>
    Task DeleteObjectAsync(string objectKey);
}
