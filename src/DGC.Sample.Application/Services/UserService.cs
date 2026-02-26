using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Mappings;
using DGC.Sample.Domain.Specifications.Users;

namespace DGC.Sample.Application.Services;

public sealed class UserService(IUserRepository userRepository, IUnitOfWork unitOfWork) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(UserMapper.ToResponse).ToArray();
    }

    public async Task<IReadOnlyList<UserResponse>> GetActiveUsersAsync(CancellationToken cancellationToken)
    {
        var spec = new UserActiveStatusSpec(true);
        var users = await _userRepository.GetListAsync(spec, cancellationToken);
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
        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
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

        _userRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return UserMapper.ToResponse(existing);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        _userRepository.Delete(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
