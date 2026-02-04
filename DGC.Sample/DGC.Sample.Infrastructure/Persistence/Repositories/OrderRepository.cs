using DGC.Sample.Application.Abstractions.Interfaces;
using DGC.Sample.Domain.Entities;
using DGC.Sample.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DGC.Sample.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _dbContext;

    public OrderRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .OrderBy(order => order.OrderDateUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken)
    {
        _dbContext.Orders.Update(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(
            existing => existing.Id == id,
            cancellationToken);

        if (order is null)
        {
            return;
        }

        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
