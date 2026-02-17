using DGC.Sample.Application.Interfaces.Repositoies;
using DGC.Sample.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace DGC.Sample.Infrastructure.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        protected DbSet<TEntity> dbSet { get; }

        public Repository(AppDbContext context)
        {
            dbSet = context.Set<TEntity>();
        }

        public virtual async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            return await AnyAsync(x => true, cancellationToken);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            var criteria = MergeWithCriteriaBase(expression);
            if (criteria == null)
            {
                return await dbSet.AnyAsync(cancellationToken);
            }
            else
            {
                return await dbSet.AnyAsync(criteria, cancellationToken);
            }
        }

        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await CountAsync(x => true, cancellationToken);
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            var criteria = MergeWithCriteriaBase(expression);
            if (criteria == null)
            {
                return await dbSet.CountAsync(cancellationToken);
            }
            else
            {
                return await dbSet.CountAsync(criteria, cancellationToken);
            }
        }

        public virtual async Task<TEntity> FindObjectAsync(
            Expression<Func<TEntity, bool>> expression,
            CancellationToken cancellationToken = default)
        {
            return await FindObjectAsync(expression, null, cancellationToken);
        }

        public virtual async Task<TEntity?> FindObjectAsync(
            Expression<Func<TEntity, bool>> expression,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> include,
            CancellationToken cancellationToken)
        {
            var query = IncludeProperties(dbSet.AsQueryable(), include);
            var criteria = MergeWithCriteriaBase(expression);
            if (criteria == null)
            {
                return await query.FirstOrDefaultAsync(cancellationToken);
            }
            else
            {
                return await query.FirstOrDefaultAsync(criteria, cancellationToken);
            }
        }

        public virtual async Task<TEntity> FindObjectByKeyAsync(
            object key,
            CancellationToken cancellationToken)
        {
            return await FindObjectByKeyAsync(key, null, cancellationToken);
        }

        public virtual async Task<TEntity> FindObjectByKeyAsync(
            object key,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> include,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(key);

            var query = IncludeProperties(dbSet.AsQueryable(), include);

            // Build a dynamic expression to find by key
            // This assumes the entity has a property named "Id" or uses a single primary key
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            // Try common primary key property names
            var propertyNames = new[] { "Id", $"{typeof(TEntity).Name}Id" };
            PropertyInfo keyProperty = null!;
            foreach (var propName in propertyNames)
            {
                keyProperty = typeof(TEntity).GetProperty(propName)!;
                if (keyProperty != null)
                    break;
            }

            if (keyProperty == null)
            {
                // Fallback: get the first property if no standard key found
                keyProperty = typeof(TEntity).GetProperties()
                    .FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))!;
            }

            if (keyProperty == null)
                throw new InvalidOperationException($"Cannot find a primary key property for entity type {typeof(TEntity).Name}");

            // Build expression: e => e.KeyProperty.Equals(key)
            var propertyAccess = Expression.Property(parameter, keyProperty);
            var keyConstant = Expression.Constant(key);
            // Convert key to property type if necessary
            var convertedKey = Expression.Convert(keyConstant, keyProperty.PropertyType);
            var equalsExpression = Expression.Equal(propertyAccess, convertedKey);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(equalsExpression, parameter);

            // Merge with security criteria
            var criteria = MergeWithCriteriaBase(lambda);

            return await query.FirstOrDefaultAsync(criteria!, cancellationToken) ?? default!;
        }

        public virtual IQueryable<TEntity> GetObjectsAsync(
            Expression<Func<TEntity, bool>>? expression = null,
            CancellationToken cancellationToken = default)
        {
            return GetObjectsAsync(expression, null, null, cancellationToken);
        }

        public virtual IQueryable<TEntity> GetObjectsAsync(
            Expression<Func<TEntity, bool>> expression,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> include,
            CancellationToken cancellationToken)
        {
            return GetObjectsAsync(expression, null, include, cancellationToken);
        }

        public virtual IQueryable<TEntity> GetObjectsAsync(
            Expression<Func<TEntity, bool>> expression,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> include,
            CancellationToken cancellationToken = default)
        {
            var criteria = MergeWithCriteriaBase(expression);

            // Start with the base query
            IQueryable<TEntity> query = dbSet.AsNoTracking();

            // Apply includes
            query = IncludeProperties(query, include);

            // Apply criteria
            if (criteria != null)
            {
                query = query.Where(criteria);
            }

            // Apply ordering
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return query;
        }

        protected virtual Expression<Func<TEntity, bool>>? MergeWithCriteriaBase(Expression<Func<TEntity, bool>>? expression)
        {
            return expression;
        }


        protected IQueryable<TEntity> IncludeProperties(IQueryable<TEntity> query, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include)
        {
            if (include != null)
            {
                query = include(query);
            }
            return query;
        }

        public virtual void Create(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            dbSet.Add(entity);
        }

        public virtual TEntity CreateEntity()
        {
            return Activator.CreateInstance<TEntity>();
        }

        public virtual void Delete(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            dbSet.Remove(entity);
        }

        public virtual void Update(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            dbSet.Update(entity);
        }

        public virtual IQueryable<TEntity> Query()
        {
            return dbSet.AsQueryable();
        }

        public virtual IQueryable<TEntity> QueryAsNoTracking()
        {
            return dbSet.AsNoTracking();
        }
    }
}
