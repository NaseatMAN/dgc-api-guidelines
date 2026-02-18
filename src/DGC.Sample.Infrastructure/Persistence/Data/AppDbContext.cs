using DGC.Sample.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DGC.Sample.Infrastructure.Persistence.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<IdempotentRequest> IdempotentRequests => Set<IdempotentRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdempotentRequest>(entity =>
        {
            entity.ToTable("idempotent_requests");
            entity.HasKey(req => req.IdempotencyKey);
            entity.Property(req => req.IdempotencyKey).HasMaxLength(128);
            entity.Property(req => req.RequestPath).HasMaxLength(500).IsRequired();
            entity.Property(req => req.ResponseBody).IsRequired();
            entity.HasIndex(req => req.IdempotencyKey).IsUnique();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(order => order.Id);
            entity.Property(order => order.CustomerName)
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(order => order.OrderDateUtc)
                .IsRequired();
            entity.Property(order => order.Status)
                .IsRequired();
            entity.Property(order => order.TotalAmount)
                .HasPrecision(18, 2)
                .IsRequired();
        });
    }
}
