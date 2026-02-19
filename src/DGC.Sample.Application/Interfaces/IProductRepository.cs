using DGC.Sample.Domain.Entities;

namespace DGC.Sample.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<int> GetAvailableStockAsync(Guid id, CancellationToken cancellationToken);
}
