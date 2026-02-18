using DGC.Sample.Application.Dtos;
using DGC.Sample.Domain.Entities;

namespace DGC.Sample.Application.Mappings;

public static class UserMapper
{
    public static UserResponse ToResponse(User user) =>
        new(user.Id, user.FullName, user.NationalId, user.PhoneNumber, user.Email, user.CreatedAtUtc);

    public static User ToEntity(Guid id, UserCreateRequest request) =>
        new()
        {
            Id = id,
            FullName = request.FullName,
            NationalId = request.NationalId,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            CreatedAtUtc = DateTime.UtcNow
        };
}
