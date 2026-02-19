using DGC.Sample.Application.Interfaces;
using DGC.Sample.Domain.Entities;
using DGC.Sample.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace DGC.Sample.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(AppDbContext dbContext) : IProductRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<int> GetAvailableStockAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .Where(p => p.Id == id)
            .Select(p => p.AvailableStock)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
