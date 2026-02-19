using DGC.Sample.Domain.Entities;

namespace DGC.Sample.Application.Interfaces;

public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken);
    Task<bool> ExistsByNationalIdAsync(string nationalId, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task UpdateAsync(User user, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
