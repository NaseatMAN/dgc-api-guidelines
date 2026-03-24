namespace DGC.Sample.Application.Dtos;

public sealed record PublicUserResponse(
    int Id,
    string Name,
    string Username,
    string Email,
    string? Phone,
    string? Website);
