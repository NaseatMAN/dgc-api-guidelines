namespace DGC.Sample.Application.Interfaces.Repositoies
{
    public interface IBaseRepository<TEntity>
        where TEntity : class
    {
        IQueryable<TEntity> Query();
        IQueryable<TEntity> QueryAsNoTracking();
    }
}
