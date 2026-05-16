using AlgoSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoSphere.Api.Controllers;

/// <summary>
/// Pre-signed URL endpoints for direct client-to-S3/MinIO uploads.
/// This keeps large file data (test cases, avatars) off the API server.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class StorageController : ControllerBase
{
    private readonly IStorageService _storage;

    public StorageController(IStorageService storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// Returns a pre-signed PUT URL.
    /// The client uploads directly to S3/MinIO — the API server is never in the data path.
    /// Usage: GET /api/v1/storage/presigned-upload?filename=testcases.zip
    /// </summary>
    [HttpGet("presigned-upload")]
    public async Task<ActionResult<PresignedUrlResponse>> GetUploadUrl([FromQuery] string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return BadRequest("filename is required.");

        // Namespace by user + timestamp to prevent collisions
        var objectKey = $"uploads/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}_{filename}";
        var url = await _storage.GetPresignedUploadUrlAsync(objectKey);

        return Ok(new PresignedUrlResponse(url, objectKey));
    }

    /// <summary>
    /// Returns a pre-signed GET URL for downloading a stored object.
    /// Usage: GET /api/v1/storage/presigned-download?key=uploads/2026/05/16/abc_testcases.zip
    /// </summary>
    [HttpGet("presigned-download")]
    public async Task<ActionResult<PresignedUrlResponse>> GetDownloadUrl([FromQuery] string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return BadRequest("key is required.");

        var url = await _storage.GetPresignedDownloadUrlAsync(key);
        return Ok(new PresignedUrlResponse(url, key));
    }
}

public record PresignedUrlResponse(string Url, string ObjectKey);
