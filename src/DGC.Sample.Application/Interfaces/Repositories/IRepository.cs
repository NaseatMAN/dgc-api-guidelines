namespace DGC.Sample.Application.Interfaces.Repositories
{
    public interface IRepository<TEntity> : IReaderRepository<TEntity>, IWriterRepository<TEntity>
        where TEntity : class
    {
    }
}
