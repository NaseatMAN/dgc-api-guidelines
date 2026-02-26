using DGC.Sample.Application.Interfaces;
using DGC.Sample.Domain.Entities;
using DGC.Sample.Infrastructure.Persistence.Data;
using DGC.Sample.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DGC.Sample.Infrastructure.Persistence.Repositories;

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

    public async Task<bool> ExistsByNationalIdAsync(string nationalId, CancellationToken cancellationToken)
    {
        return await Query().AnyAsync(u => u.NationalId == nationalId, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        _dbSet.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user != null)
        {
            _dbSet.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
