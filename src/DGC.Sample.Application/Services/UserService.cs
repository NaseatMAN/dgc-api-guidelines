using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Mappings;

namespace DGC.Sample.Application.Services;

public sealed class UserService(IUserRepository userRepository) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(UserMapper.ToResponse).ToArray();
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : UserMapper.ToResponse(user);
    }

    public async Task<UserResponse> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken)
    {
        var user = UserMapper.ToEntity(Guid.NewGuid(), request);
        await _userRepository.AddAsync(user, cancellationToken);
        return UserMapper.ToResponse(user);
    }

    public async Task<UserResponse?> UpdateAsync(Guid id, UserUpdateRequest request, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.FullName = request.FullName;
        existing.NationalId = request.NationalId;
        existing.PhoneNumber = request.PhoneNumber;
        existing.Email = request.Email;

        await _userRepository.UpdateAsync(existing, cancellationToken);
        return UserMapper.ToResponse(existing);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        await _userRepository.DeleteAsync(id, cancellationToken);
        return true;
    }
}
