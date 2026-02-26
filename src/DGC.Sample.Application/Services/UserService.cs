using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Interfaces.Persistence;
using DGC.Sample.Application.Interfaces.Repositories;
using DGC.Sample.Application.Mappings;
using DGC.Sample.Domain.Entities;
using DGC.Sample.Domain.Specifications.Users;

namespace DGC.Sample.Application.Services;

public sealed class UserService(IUnitOfWork unitOfWork) : IUserRepository
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entityRepository = _unitOfWork.GetEntityRepository<User>();
        var users = entityRepository.QueryAsNoTracking().ToList();
        return users.Select(UserMapper.ToResponse).ToArray();
    }

    public async Task<IReadOnlyList<UserResponse>> GetActiveUsersAsync(CancellationToken cancellationToken)
    {
        var spec = new UserActiveStatusSpec(true);
        var entityRepository = _unitOfWork.GetEntityRepository<User>();
        var users = await entityRepository.GetListAsync(spec, cancellationToken);
        return users.Select(UserMapper.ToResponse).ToArray();
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entityRepository = _unitOfWork.GetEntityRepository<User>();
        var user = await entityRepository.FindFirstAsync(u => u.Id == id, cancellationToken);
        return user is null ? null : UserMapper.ToResponse(user);
    }

    public async Task<UserResponse> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken)
    {
        var entityRepository = _unitOfWork.GetEntityRepository<User>();
        var user = UserMapper.ToEntity(Guid.NewGuid(), request);
        entityRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return UserMapper.ToResponse(user);
    }

    public async Task<UserResponse?> UpdateAsync(Guid id, UserUpdateRequest request, CancellationToken cancellationToken)
    {
        var entityRepository = _unitOfWork.GetEntityRepository<User>();
        var existing = await entityRepository.FindFirstAsync(u => u.Id == id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.FullName = request.FullName;
        existing.NationalId = request.NationalId;
        existing.PhoneNumber = request.PhoneNumber;
        existing.Email = request.Email;

        entityRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return UserMapper.ToResponse(existing);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entityRepository = _unitOfWork.GetEntityRepository<User>();
        var existing = await entityRepository.FindFirstAsync(u => u.Id == id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        entityRepository.Delete(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
