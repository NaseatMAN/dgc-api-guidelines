

using DGC.Sample.Domain.Entities;

namespace DGC.Sample.Application.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
}
