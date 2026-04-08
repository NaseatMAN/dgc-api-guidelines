namespace DGC.Sample.Application.Dtos;

public sealed record UploadFileInputDto(
    string FileName,
    string? ContentType,
    byte[] Content);

public sealed record UploadResultDto(string BlobName);

public sealed record PrivateBlobResultDto(
    string BlobName,
    string ContentType,
    byte[] Content);

public sealed record BlobTextResultDto(
    string BlobName,
    string Content);