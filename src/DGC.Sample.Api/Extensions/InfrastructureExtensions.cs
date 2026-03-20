using DGC.Sample.Application.Interfaces.Persistence;
using DGC.Sample.Application.Interfaces.Repositories;
using DGC.Sample.Infrastructure.Caching;
using DGC.Sample.Infrastructure.Persistence.Data;
using DGC.Sample.Infrastructure.Persistence.Repositories.Purchases;
using DGC.Sample.Infrastructure.Persistence.Repositories.UserMgmt;
using DGC.Sample.Infrastructure.Persistence.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace DGC.Sample.Api.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddApiInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresqlServer(configuration);
        // services.AddSqlServer(configuration);

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddExternalApiHttpClients(configuration);

        services.AddHybridCache();
        services.AddScoped<IIdempotencyService, HybridCacheIdempotencyService>();

        return services;
    }

    public static async Task<WebApplication> ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        return app;
    }
}
