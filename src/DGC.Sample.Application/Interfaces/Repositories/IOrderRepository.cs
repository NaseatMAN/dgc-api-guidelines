

using DGC.Sample.Domain.Entities;

namespace DGC.Sample.Application.Interfaces.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Order?> GetWithItemsByIdAsync(Guid id, CancellationToken cancellationToken);
}
