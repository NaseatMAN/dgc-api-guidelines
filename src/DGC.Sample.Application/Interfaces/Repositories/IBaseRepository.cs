namespace DGC.Sample.Application.Interfaces.Repositories
{
    public interface IBaseRepository<TEntity>
        where TEntity : class
    {
        IQueryable<TEntity> Query();
        IQueryable<TEntity> QueryAsNoTracking();
    }
}
