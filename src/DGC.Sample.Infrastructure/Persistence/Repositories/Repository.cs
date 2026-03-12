using DGC.Sample.Application.Interfaces.Repositories;
using DGC.Sample.Domain.Specifications;
using DGC.Sample.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DGC.Sample.Infrastructure.Persistence.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        protected readonly DbSet<TEntity> DbSet;
        protected readonly AppDbContext Context;

        public Repository(AppDbContext context)
        {
            Context = context;
            DbSet = context.Set<TEntity>();
        }

        public virtual ValueTask<TEntity?> FindByKeyAsync(CancellationToken ct, params object[] keyValues)
        {
            return DbSet.FindAsync(keyValues, ct);
        }

        public virtual Task<TEntity?> FindFirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
        {
            return DbSet.FirstOrDefaultAsync(predicate, ct);
        }

        public virtual async Task<TEntity?> FindFirstAsync(ISpecification<TEntity> specification, CancellationToken ct)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(ct);
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetListAsync(ISpecification<TEntity> specification, CancellationToken ct)
        {
            return await ApplySpecification(specification).ToListAsync(ct);
        }

        public virtual Task<bool> AnyAsync(CancellationToken ct)
        {
            return DbSet.AnyAsync(ct);
        }

        public virtual Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
        {
            return DbSet.AnyAsync(predicate, ct);
        }

        public virtual Task<int> CountAsync(CancellationToken ct)
        {
            return DbSet.CountAsync(ct);
        }

        public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
        {
            return DbSet.CountAsync(predicate, ct);
        }

        public virtual void Add(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            DbSet.Add(entity);
        }

        public virtual void Update(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            DbSet.Update(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            DbSet.Remove(entity);
        }

        public virtual TEntity CreateEntity()
        {
            return Activator.CreateInstance<TEntity>();
        }

        public virtual IQueryable<TEntity> Query()
        {
            return DbSet.AsQueryable();
        }

        public virtual IQueryable<TEntity> QueryAsNoTracking()
        {
            return DbSet.AsNoTracking();
        }

        protected IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
        {
            var query = DbSet.AsQueryable();

            // 1. Toggling Named Filters (EF Core 10 feature)
            if (specification.IgnoredNamedFilters.Count > 0)
            {
                query = query.IgnoreQueryFilters([.. specification.IgnoredNamedFilters]);
            }

            // 2. Criteria
            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            // 3. Includes
            query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

            // 4. Ordering
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            return query;
        }
    }
}
