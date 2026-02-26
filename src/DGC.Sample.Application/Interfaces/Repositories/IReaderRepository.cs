using System.Linq.Expressions;
using DGC.Sample.Domain.Specifications;

namespace DGC.Sample.Application.Interfaces.Repositories
{
    public interface IReaderRepository<TEntity> : IBaseRepository<TEntity>
        where TEntity : class
    {
        ValueTask<TEntity?> FindByKeyAsync(CancellationToken cancellationToken, params object[] keyValues);
        Task<TEntity?> FindFirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
        Task<TEntity?> FindFirstAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken);
        Task<IReadOnlyList<TEntity>> GetListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken);
        Task<bool> AnyAsync(CancellationToken cancellationToken);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
        Task<int> CountAsync(CancellationToken cancellationToken);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    }
}
