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
    [property: NotWhiteSpace]
    [property: StringLengthRange(1, 200)]
    string FullName,
    [property: CambodiaNationalId]
    string NationalId,
    [property: CambodiaPhoneNumber]
    string PhoneNumber,
    [property: RequiredEmail]
    string Email);

public sealed record UserUpdateRequest(
    [property: NotWhiteSpace]
    [property: StringLengthRange(1, 200)]
    string FullName,
    [property: CambodiaNationalId]
    string NationalId,
    [property: CambodiaPhoneNumber]
    string PhoneNumber,
    [property: RequiredEmail]
    string Email);
