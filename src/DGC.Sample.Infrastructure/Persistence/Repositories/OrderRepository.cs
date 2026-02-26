using DGC.Sample.Application.Interfaces;
using DGC.Sample.Domain.Entities;
using DGC.Sample.Domain.Specifications.Orders;
using DGC.Sample.Infrastructure.Persistence.Data;
using DGC.Sample.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DGC.Sample.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(AppDbContext dbContext) : Repository<Order>(dbContext)
{
    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await QueryAsNoTracking()
            .OrderBy(order => order.OrderDateUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Query()
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public async Task<Order?> GetWithItemsByIdAsync(Guid id, CancellationToken ct)
    {
        var spec = new OrderWithItemsSpec(id);
        return await ApplySpecification(spec).FirstOrDefaultAsync(ct);
    }
}
