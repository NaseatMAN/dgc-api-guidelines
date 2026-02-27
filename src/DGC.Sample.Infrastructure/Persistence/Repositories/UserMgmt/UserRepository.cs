using DGC.Sample.Application.Interfaces.Repositories;
using DGC.Sample.Domain.Entities;
using DGC.Sample.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace DGC.Sample.Infrastructure.Persistence.Repositories.UserMgmt;
public sealed class UserRepository(AppDbContext dbContext) : Repository<User>(dbContext), IUserRepository
{
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await QueryAsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await Query().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken)
    {
        return await Query().FirstOrDefaultAsync(u => u.NationalId == nationalId, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await Query().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}
