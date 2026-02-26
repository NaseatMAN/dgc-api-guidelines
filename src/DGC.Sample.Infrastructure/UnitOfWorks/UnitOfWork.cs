using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Interfaces.Persistence;
using DGC.Sample.Application.Interfaces.Repositories;
using DGC.Sample.Infrastructure.Persistence.Data;
using DGC.Sample.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DGC.Sample.Infrastructure.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext context;
        private readonly IServiceProvider serviceProvider;
        private bool disposed = false;

        public UnitOfWork(AppDbContext dbContext, IServiceProvider serviceProvider)
        {
            this.context = dbContext;
            this.serviceProvider = serviceProvider;
        }

        public TRepository GetRepository<TRepository>()
            where TRepository : class
        {
            var repository = serviceProvider.GetService<TRepository>();
            ArgumentNullException.ThrowIfNull(repository, $"Repository of type {typeof(TRepository).FullName} is not registered.");

            return repository;
        }

        public IRepository<TEntity> GetEntityRepository<TEntity>() where TEntity : class
        {
            return new Repository<TEntity>(context);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("An error occurred while saving changes to the database.", ex);
            }
        }

        public void DiscardChanges()
        {
            foreach (var entry in context.ChangeTracker.Entries())
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
            return context.ChangeTracker.HasChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                context.Dispose();
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
