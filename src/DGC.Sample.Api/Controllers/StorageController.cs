using Asp.Versioning;
using DGC.Sample.Api.Attributes;
using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace DGC.Sample.Api.Controllers;

[ApiController]
[ApiVersion("2026-02-05")]
[Route("storage")]
public sealed class StorageController(IStorageService storageService) : ControllerBase
{
    private readonly IStorageService _storageService = storageService;

    [HttpPost("upload")]
    // Sample API-key auth: keep or remove this attribute in code depending on whether the endpoint should require API-key authentication.
    // [ApiKeyAuthentication("storage-write")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);

        var result = await _storageService.UploadFileAsync(
            new UploadFileInputDto(file.FileName, file.ContentType, memoryStream.ToArray()),
            cancellationToken);

        return CreatedAtAction(nameof(Download), new { blobName = result.BlobName }, result);
    }

    [HttpGet("{blobName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(string blobName, CancellationToken cancellationToken)
    {
        var blob = await _storageService.GetBlobAsync(blobName, cancellationToken);
        return File(blob.Content, blob.ContentType, blob.BlobName);
    }

    [HttpGet("{blobName}/text")]
    // Sample API-key auth: a different key group can protect a different endpoint without sharing the same secret set.
    // This can also be combined with [Authorize(...)] on the same action when business rules require both checks.
    // [ApiKeyAuthentication("storage-read")]
    [ProducesResponseType(typeof(BlobTextResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReadText(string blobName, CancellationToken cancellationToken)
    {
        var blob = await _storageService.GetBlobAsTextAsync(blobName, cancellationToken);
        return Ok(blob);
    }
}