using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces.Storage;
using DGC.Sample.Domain.Constants.ApiErrorConstants;
using DGC.Sample.Domain.Exceptions;
using Microsoft.Extensions.Configuration;

namespace DGC.Sample.Infrastructure.Storage;

public sealed class FileStorageService(IConfiguration configuration) : IFileStorageService
{
    private readonly string _blobStorageConnectionString = configuration.GetConnectionString("BlobStorage")
        ?? throw new InvalidOperationException("Connection string 'ConnectionStrings:BlobStorage' is required.");

    private readonly string _containerName = configuration["Storage:Blob:ContainerName"] ?? "files";

    public async Task<UploadResultDto> UploadFileAsync(IReadOnlyList<UploadFileInputDto> files, CancellationToken cancellationToken = default)
    {
        if (files.Count == 0)
        {
            throw new BadRequestException(BadRequestErrorCode.InvalidModelError, "No file uploaded.", azureErrorDetails: null);
        }

        var file = files[0];
        if (file.Content.Length == 0)
        {
            throw new BadRequestException(BadRequestErrorCode.InvalidModelError, "File is empty.", azureErrorDetails: null);
        }

        var blobName = $"{Guid.NewGuid():N}-{file.FileName}";

        var containerClient = await GetContainerClientAsync(cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);

        using var contentStream = new MemoryStream(file.Content, writable: false);
        await blobClient.UploadAsync(
            contentStream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                        ? ResolveContentType(file.FileName)
                        : file.ContentType
                }
            },
            cancellationToken);

        return new UploadResultDto(blobName);
    }

    public async Task<PrivateBlobResultDto> GetBlobAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var blobClient = (await GetContainerClientAsync(cancellationToken)).GetBlobClient(blobName);
        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            throw new NotFoundException(NotFoundErrorCode.ItemNotFound, $"Blob {blobName} not found.", nameof(blobName), azureErrorDetails: null);
        }

        var download = await blobClient.DownloadContentAsync(cancellationToken);
        var content = download.Value.Content.ToMemory().ToArray();
        var contentType = download.Value.Details.ContentType ?? ResolveContentType(blobName);

        return new PrivateBlobResultDto(blobName, contentType, content);
    }

    public async Task<BlobTextResultDto> GetBlobAsTextAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var blobClient = (await GetContainerClientAsync(cancellationToken)).GetBlobClient(blobName);
        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            throw new NotFoundException(NotFoundErrorCode.ItemNotFound, $"Blob {blobName} not found.", nameof(blobName), azureErrorDetails: null);
        }

        var download = await blobClient.DownloadContentAsync(cancellationToken);
        var content = download.Value.Content.ToMemory();

        return new BlobTextResultDto(blobName, Encoding.UTF8.GetString(content.Span));
    }

    private async Task<BlobContainerClient> GetContainerClientAsync(CancellationToken cancellationToken)
    {
        var containerClient = new BlobContainerClient(_blobStorageConnectionString, _containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        return containerClient;
    }

    private static string ResolveContentType(string blobName)
    {
        if (blobName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) return "image/png";
        if (blobName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || blobName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)) return "image/jpeg";
        if (blobName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) return "text/plain";
        return "application/octet-stream";
    }
}
