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
        protected AppDbContext context { get; }

        public Repository(AppDbContext context)
        {
            this.context = context;
            dbSet = context.Set<TEntity>();
        }

        public virtual async Task<bool> AnyAsync(CancellationToken cancellationToken)
        {
            return await AnyAsync(null, cancellationToken);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? expression, CancellationToken cancellationToken)
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

        public virtual async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            return await CountAsync(null, cancellationToken);
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? expression, CancellationToken cancellationToken)
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

        public virtual async Task<TEntity?> FindObjectAsync(
            Expression<Func<TEntity, bool>> expression,
            CancellationToken cancellationToken)
        {
            return await FindObjectAsync(expression, null, cancellationToken);
        }

        public virtual async Task<TEntity?> FindObjectAsync(
            Expression<Func<TEntity, bool>> expression,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include,
            CancellationToken cancellationToken)
        {
            var query = IncludeProperties(Query(), include);
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

        public virtual async Task<TEntity?> FindObjectByKeyAsync(
            object key,
            CancellationToken cancellationToken)
        {
            return await FindObjectByKeyAsync(key, null, cancellationToken);
        }

        public virtual async Task<TEntity?> FindObjectByKeyAsync(
            object key,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(key);

            var entityType = context.Model.FindEntityType(typeof(TEntity)) ?? throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} is not found in the model.");
            var primaryKey = entityType.FindPrimaryKey() ?? throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} has no primary key defined.");
            var keyProperties = primaryKey.Properties;
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            Expression? predicate = null;

            if (keyProperties.Count == 1)
            {
                var property = keyProperties[0];
                var propertyAccess = Expression.Property(parameter, property.PropertyInfo!);

                // Build expression: e => e.KeyProperty.Equals(key)
                var convertedKeyValue = ConvertKey(key, property.ClrType);
                var keyConstant = Expression.Constant(convertedKeyValue, property.ClrType);
                predicate = Expression.Equal(propertyAccess, keyConstant);
            }
            else
            {
                // Composite key support: key can be an object with matching property names,
                // IDictionary<string, object>, or object[] (matched by index)

                for (int i = 0; i < keyProperties.Count; i++)
                {
                    var property = keyProperties[i];
                    object? keyValue = null;

                    if (key is System.Collections.IDictionary dict)
                    {
                        keyValue = dict[property.Name];
                    }
                    else if (key is object[] objects && objects.Length == keyProperties.Count)
                    {
                        keyValue = objects[i];
                    }
                    else
                    {
                        keyValue = key.GetType().GetProperty(property.Name)?.GetValue(key);
                    }

                    if (keyValue == null)
                        throw new ArgumentException($"Value for key property '{property.Name}' was not found in the provided key.");

                    var propertyAccess = Expression.Property(parameter, property.PropertyInfo!);
                    var convertedKeyValue = ConvertKey(keyValue, property.ClrType);
                    var keyConstant = Expression.Constant(convertedKeyValue, property.ClrType);
                    var equals = Expression.Equal(propertyAccess, keyConstant);

                    predicate = predicate == null ? equals : Expression.AndAlso(predicate, equals);
                }
            }

            var lambda = Expression.Lambda<Func<TEntity, bool>>(predicate!, parameter);

            // Merge with security criteria
            var query = IncludeProperties(Query(), include);
            var criteria = MergeWithCriteriaBase(lambda) ?? (x => true);

            return await query.FirstOrDefaultAsync(criteria, cancellationToken);
        }

        private static object? ConvertKey(object? value, Type targetType)
        {
            if (value == null) return null;
            if (targetType.IsAssignableFrom(value.GetType())) return value;

            try
            {
                var converter = System.ComponentModel.TypeDescriptor.GetConverter(targetType);
                if (converter.CanConvertFrom(value.GetType()))
                {
                    return converter.ConvertFrom(value);
                }
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                // Fallback to original value, let EF handle it or fail later
                return value;
            }
        }

        public virtual IQueryable<TEntity> GetObjectsAsync(
            Expression<Func<TEntity, bool>>? expression,
            CancellationToken cancellationToken)
        {
            return GetObjectsAsync(expression, null, null, cancellationToken);
        }

        public virtual IQueryable<TEntity> GetObjectsAsync(
            Expression<Func<TEntity, bool>>? expression,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include,
            CancellationToken cancellationToken)
        {
            return GetObjectsAsync(expression, null, include, cancellationToken);
        }

        public virtual IQueryable<TEntity> GetObjectsAsync(
            Expression<Func<TEntity, bool>>? expression,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include,
            CancellationToken cancellationToken)
        {
            var criteria = MergeWithCriteriaBase(expression);

            // Start with the base query
            IQueryable<TEntity> query = QueryAsNoTracking();

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
