using DGC.Sample.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DGC.Sample.Infrastructure.Persistence.Data;

public partial class AppDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Name)
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(product => product.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();
            entity.Property(product => product.AvailableStock)
                .IsRequired();
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

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.FullName)
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(user => user.NationalId)
                .HasMaxLength(10)
                .IsRequired();
            entity.HasIndex(user => user.NationalId).IsUnique();
            entity.Property(user => user.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(user => user.Email)
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(user => user.CreatedAtUtc)
                .IsRequired();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
