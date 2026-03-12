using DGC.Sample.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DGC.Sample.Infrastructure.Persistence.Data;

public partial class AppDbContext : DbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            // Keep soft-delete behavior without tenant scoping.
            if (typeof(ISoftDeletable).IsAssignableFrom(clrType))
            {
                var parameter = Expression.Parameter(clrType, "e");
                var property = Expression.Property(Expression.Convert(parameter, typeof(ISoftDeletable)), nameof(ISoftDeletable.IsDeleted));
                var comparison = Expression.Equal(property, Expression.Constant(false));
                var lambda = Expression.Lambda(comparison, parameter);

                modelBuilder.Entity(clrType).HasQueryFilter("SoftDeleteFilter", lambda);
            }
        }
    }
}
