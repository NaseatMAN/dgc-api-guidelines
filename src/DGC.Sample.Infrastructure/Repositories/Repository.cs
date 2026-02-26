using DGC.Sample.Application.Interfaces.Repositories;
using DGC.Sample.Domain.Specifications;
using DGC.Sample.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DGC.Sample.Infrastructure.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        protected readonly DbSet<TEntity> _dbSet;
        protected readonly AppDbContext _context;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public virtual ValueTask<TEntity?> FindByKeyAsync(CancellationToken ct, params object[] keyValues)
        {
            return _dbSet.FindAsync(keyValues, ct);
        }

        public virtual Task<TEntity?> FindFirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
        {
            return _dbSet.FirstOrDefaultAsync(predicate, ct);
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
            return _dbSet.AnyAsync(ct);
        }

        public virtual Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
        {
            return _dbSet.AnyAsync(predicate, ct);
        }

        public virtual Task<int> CountAsync(CancellationToken ct)
        {
            return _dbSet.CountAsync(ct);
        }

        public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
        {
            return _dbSet.CountAsync(predicate, ct);
        }

        public virtual void Add(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Add(entity);
        }

        public virtual void Update(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Update(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Remove(entity);
        }

        public virtual TEntity CreateEntity()
        {
            return Activator.CreateInstance<TEntity>();
        }

        public virtual IQueryable<TEntity> Query()
        {
            return _dbSet.AsQueryable();
        }

        public virtual IQueryable<TEntity> QueryAsNoTracking()
        {
            return _dbSet.AsNoTracking();
        }

        protected IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
        {
            var query = _dbSet.AsQueryable();

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
