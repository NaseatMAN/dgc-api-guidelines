using DGC.Sample.Application.Interfaces.Repositories;
using DGC.Sample.Domain.Entities;

namespace DGC.Sample.Application.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken);
}
