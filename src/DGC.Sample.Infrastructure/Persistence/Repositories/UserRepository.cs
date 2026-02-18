using DGC.Sample.Application.Interfaces;
using DGC.Sample.Domain.Entities;
using DGC.Sample.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace DGC.Sample.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Users.ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.NationalId == nationalId, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user != null)
        {
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
