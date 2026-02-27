using DGC.Sample.Application.Dtos;

namespace DGC.Sample.Application.Interfaces.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<UserResponse>> GetActiveUsersAsync(CancellationToken cancellationToken);
    Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<UserResponse> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken);
    Task<UserResponse?> UpdateAsync(Guid id, UserUpdateRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
