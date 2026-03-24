using DGC.Sample.Application.Validators;

namespace DGC.Sample.Application.Dtos;

public sealed record UserResponse(
    Guid Id,
    string FullName,
    string NationalId,
    string PhoneNumber,
    string Email,
    DateTime CreatedAtUtc);

public sealed record UserCreateRequest(
    [NotWhiteSpace]
    [StringLengthRange(1, 200)]
    string FullName,
    [CambodiaNationalId]
    string NationalId,
    [CambodiaPhoneNumber]
    string PhoneNumber,
    [RequiredEmail]
    string Email);

public sealed record UserUpdateRequest(
    [NotWhiteSpace]
    [StringLengthRange(1, 200)]
    string FullName,
    [CambodiaNationalId]
    string NationalId,
    [CambodiaPhoneNumber]
    string PhoneNumber,
    [RequiredEmail]
    string Email);
