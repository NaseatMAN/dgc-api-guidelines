using System.Linq.Expressions;

namespace DGC.Sample.Application.Interfaces.Repositories
{
    public interface IReaderRepository<TEntity> : IBaseRepository<TEntity>
        where TEntity : class
    {
        Task<TEntity?> FindObjectByKeyAsync(object key, CancellationToken cancellationToken);
        Task<TEntity?> FindObjectByKeyAsync(object key, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include, CancellationToken cancellationToken);
        Task<TEntity?> FindObjectAsync(Expression<Func<TEntity, bool>> expression, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include, CancellationToken cancellationToken);
        Task<TEntity?> FindObjectAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken);
        IQueryable<TEntity> GetObjectsAsync(Expression<Func<TEntity, bool>>? expression, CancellationToken cancellationToken);
        IQueryable<TEntity> GetObjectsAsync(Expression<Func<TEntity, bool>>? expression, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include, CancellationToken cancellationToken);
        IQueryable<TEntity> GetObjectsAsync(Expression<Func<TEntity, bool>>? expression, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include, CancellationToken cancellationToken);
        Task<bool> AnyAsync(CancellationToken cancellationToken);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? expression, CancellationToken cancellationToken);
        Task<int> CountAsync(CancellationToken cancellationToken);
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? expression, CancellationToken cancellationToken);
    }
}
