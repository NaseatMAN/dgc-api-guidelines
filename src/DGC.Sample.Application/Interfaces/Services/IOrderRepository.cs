using DGC.Sample.Application.Interfaces.Repositories;
using DGC.Sample.Domain.Entities;

namespace DGC.Sample.Application.Interfaces.Services;
  
public interface IOrderRepository : IRepository<Order>
{
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Order?> GetWithItemsByIdAsync(Guid id, CancellationToken ct);
}
