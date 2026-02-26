using DGC.Sample.Application.Interfaces;
using DGC.Sample.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DGC.Sample.Infrastructure.Persistence.Data;

public partial class AppDbContext : DbContext
{
    private readonly string _currentTenantId;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantAccessor tenantAccessor) 
        : base(options)
    {
        _currentTenantId = tenantAccessor.TenantId;
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            // 1. Named Tenancy Filter
            if (typeof(ITenantEntity).IsAssignableFrom(clrType))
            {
                var parameter = Expression.Parameter(clrType, "e");
                var property = Expression.Property(Expression.Convert(parameter, typeof(ITenantEntity)), nameof(ITenantEntity.TenantId));
                var comparison = Expression.Equal(property, Expression.Constant(_currentTenantId));
                var lambda = Expression.Lambda(comparison, parameter);

                modelBuilder.Entity(clrType).HasQueryFilter("TenantFilter", lambda);
            }

            // 2. Named Soft Delete Filter
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
