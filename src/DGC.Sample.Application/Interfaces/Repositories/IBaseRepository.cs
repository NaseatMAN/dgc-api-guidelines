using DGC.Sample.Application.Common;

namespace DGC.Sample.Application.Interfaces.Repositories
{
    public interface IBaseRepository<TEntity>
        where TEntity : class
    {
        IQueryable<TEntity> Query();
        IQueryable<TEntity> QueryAsNoTracking();
        Task<OffsetPage<TEntity>> GetPagedAsync(
            IQueryable<TEntity> query,
            int offset,
            int limit,
            CancellationToken cancellationToken);
    }
}
