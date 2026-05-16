using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using AlgoSphere.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AlgoSphere.Infrastructure.Storage;

/// <summary>
/// S3-compatible storage service.
/// Works with MinIO (local/dev) and AWS S3 (production) via the same AWS SDK.
///
/// appsettings.json (dev — MinIO):
///   "Storage": {
///     "ServiceUrl": "http://localhost:9000",
///     "AccessKey": "minioadmin",
///     "SecretKey": "minioadmin",
///     "BucketName": "algosphere"
///   }
///
/// appsettings.Production.json (AWS S3 — no ServiceUrl needed):
///   "Storage": {
///     "AccessKey": "AKIA...",
///     "SecretKey": "...",
///     "BucketName": "algosphere-prod",
///     "Region": "ap-southeast-1"
///   }
/// </summary>
public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;

    public S3StorageService(IConfiguration configuration)
    {
        var section = configuration.GetSection("Storage");
        _bucketName = section["BucketName"] ?? "algosphere";

        var accessKey = section["AccessKey"] ?? string.Empty;
        var secretKey = section["SecretKey"] ?? string.Empty;
        var serviceUrl = section["ServiceUrl"]; // null → use AWS endpoint
        var regionStr  = section["Region"] ?? "ap-southeast-1";

        var credentials = new BasicAWSCredentials(accessKey, secretKey);

        if (!string.IsNullOrEmpty(serviceUrl))
        {
            // MinIO / custom S3-compatible endpoint
            _s3 = new AmazonS3Client(credentials, new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                ForcePathStyle = true,        // MinIO requires path-style
                UseHttp = serviceUrl.StartsWith("http://")
            });
        }
        else
        {
            // AWS S3 with regional endpoint
            _s3 = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(regionStr));
        }
    }

    public async Task<string> GetPresignedUploadUrlAsync(string objectKey, int expirySeconds = 3600)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddSeconds(expirySeconds),
            ContentType = "application/octet-stream"
        };

        return await Task.FromResult(_s3.GetPreSignedURL(request));
    }

    public async Task<string> GetPresignedDownloadUrlAsync(string objectKey, int expirySeconds = 3600)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddSeconds(expirySeconds)
        };

        return await Task.FromResult(_s3.GetPreSignedURL(request));
    }

    public async Task DeleteObjectAsync(string objectKey)
    {
        await _s3.DeleteObjectAsync(_bucketName, objectKey);
    }
}
