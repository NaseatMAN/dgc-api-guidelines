using DGC.Sample.Application.Interfaces.Repositories;
using DGC.Sample.Domain.Entities;

namespace DGC.Sample.Application.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
