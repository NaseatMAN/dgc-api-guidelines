using DGC.Sample.Application.Interfaces.Repositories;

namespace DGC.Sample.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Repository access
        T GetRepository<T>() where T : class, IBaseRepository<T>;
        IRepository<T> GetEntityRepository<T>() where T : class;

        // Unit of Work operations
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        // Rollback changes
        void DiscardChanges();
    }
}
