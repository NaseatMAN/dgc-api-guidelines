namespace DGC.Sample.Application.Dtos;

public sealed record UserResponse(
    Guid Id,
    string FullName,
    string NationalId,
    string PhoneNumber,
    string Email,
    DateTime CreatedAtUtc);

public sealed record UserCreateRequest(
    string FullName,
    string NationalId,
    string PhoneNumber,
    string Email);

public sealed record UserUpdateRequest(
    string FullName,
    string NationalId,
    string PhoneNumber,
    string Email);
