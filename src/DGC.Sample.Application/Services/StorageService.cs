using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces.Services;
using DGC.Sample.Application.Interfaces.Storage;

namespace DGC.Sample.Application.Services;

public sealed class StorageService(IFileStorageService fileStorageService) : IStorageService
{
    private readonly IFileStorageService _fileStorageService = fileStorageService;

    public Task<UploadResultDto> UploadFileAsync(UploadFileInputDto file, CancellationToken cancellationToken)
    {
        return _fileStorageService.UploadFileAsync([file], cancellationToken);
    }

    public Task<PrivateBlobResultDto> GetBlobAsync(string blobName, CancellationToken cancellationToken)
    {
        return _fileStorageService.GetBlobAsync(blobName, cancellationToken);
    }

    public Task<BlobTextResultDto> GetBlobAsTextAsync(string blobName, CancellationToken cancellationToken)
    {
        return _fileStorageService.GetBlobAsTextAsync(blobName, cancellationToken);
    }
}