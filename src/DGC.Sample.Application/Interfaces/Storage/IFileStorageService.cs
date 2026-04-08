using DGC.Sample.Application.Dtos;

namespace DGC.Sample.Application.Interfaces.Storage;

public interface IFileStorageService
{
    Task<UploadResultDto> UploadFileAsync(IReadOnlyList<UploadFileInputDto> files, CancellationToken cancellationToken = default);
    Task<PrivateBlobResultDto> GetBlobAsync(string blobName, CancellationToken cancellationToken = default);
    Task<BlobTextResultDto> GetBlobAsTextAsync(string blobName, CancellationToken cancellationToken = default);
}