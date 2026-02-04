using DGC.Sample.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DGC.Sample.Infrastructure.Persistence.Context;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
