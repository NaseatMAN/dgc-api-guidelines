using DGC.Sample.Application.Dtos;

namespace DGC.Sample.Application.Interfaces.Services;

public interface IStorageService
{
    Task<UploadResultDto> UploadFileAsync(UploadFileInputDto file, CancellationToken cancellationToken);
    Task<PrivateBlobResultDto> GetBlobAsync(string blobName, CancellationToken cancellationToken);
    Task<BlobTextResultDto> GetBlobAsTextAsync(string blobName, CancellationToken cancellationToken);
}