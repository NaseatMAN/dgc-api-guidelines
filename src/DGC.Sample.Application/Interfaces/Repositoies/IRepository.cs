namespace DGC.Sample.Application.Interfaces.Repositoies
{
    public interface IRepository<TEntity> : IReaderRepository<TEntity>, IWriterRepository<TEntity>
        where TEntity : class
    {
    }
}
