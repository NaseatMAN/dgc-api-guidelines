namespace DGC.Sample.Application.Interfaces.Repositoies
{
    public interface IWriterRepository<TEntity> : IBaseRepository<TEntity>
        where TEntity : class
    {
        //Task AddAsync(TEntity entity, CancellationToken cancellationToken);
        //Task UpdateAsync(TEntity entity, CancellationToken cancellationToken);
        //Task DeleteAsync(TEntity entity, CancellationToken cancellationToken);
        TEntity CreateEntity();
        void Create(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
    }
}
