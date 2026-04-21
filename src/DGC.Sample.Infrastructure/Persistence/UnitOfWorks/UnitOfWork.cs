using DGC.Sample.Application.Interfaces.Persistence;
using DGC.Sample.Application.Interfaces.Repositories;
using DGC.Sample.Infrastructure.Persistence.Data;
using DGC.Sample.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DGC.Sample.Infrastructure.Persistence.UnitOfWorks
{
    public class UnitOfWork(AppDbContext dbContext, IServiceProvider serviceProvider) : IUnitOfWork
    {
        private bool _disposed;

        public TRepository GetRepository<TRepository>()
            where TRepository : class
        {
            var repository = serviceProvider.GetService<TRepository>();
            ArgumentNullException.ThrowIfNull(repository, $"Repository of type {typeof(TRepository).FullName} is not registered.");

            return repository;
        }

        public IRepository<TEntity> GetEntityRepository<TEntity>() where TEntity : class
        {
            return new Repository<TEntity>(dbContext);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("An error occurred while saving changes to the database.", ex);
            }
        }

        public void DiscardChanges()
        {
            foreach (var entry in dbContext.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                }
            }
        }

        public bool HasUnsavedChanges()
        {
            return dbContext.ChangeTracker.HasChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                dbContext.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
