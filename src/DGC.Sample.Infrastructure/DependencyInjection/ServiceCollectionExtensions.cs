using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Queue;
using DGC.Sample.Application.Queue.Exceptions;
using DGC.Sample.Infrastructure.Persistence;
using DGC.Sample.Infrastructure.Persistence.Data;
using DGC.Sample.Infrastructure.Persistence.Repositories;
using DGC.Sample.Infrastructure.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace DGC.Sample.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var defaultConnection =
            configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=dgc_sample;Username=postgres;Password=password";

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(defaultConnection));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IIdempotencyService, IdempotencyService>();

        services.AddQueueServices(configuration);

        return services;
    }

    public static IServiceCollection AddQueueServices(this IServiceCollection services, IConfiguration configuration)
    {
        var defaultTransportRaw = configuration["Queue:DefaultTransport"];
        var defaultTransport = Enum.TryParse<QueueTransport>(defaultTransportRaw, true, out var parsedTransport)
            ? parsedTransport
            : QueueTransport.InMemory;

        services.TryAddSingleton(new QueueServiceOptions
        {
            DefaultTransport = defaultTransport
        });

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton(typeof(IMessageQueueTransport<>), typeof(InMemoryMessageQueueTransport<>)));

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.TryAddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
            services.AddRedisMessageTransport();
        }

        services.TryAddSingleton(typeof(ITransportResolver<>), typeof(TransportResolver<>));
        services.TryAddSingleton<IQueueService, QueueService>();

        return services;
    }

    public static IServiceCollection AddRedisMessageTransport(this IServiceCollection services)
    {
        if (!services.Any(descriptor => descriptor.ServiceType == typeof(IConnectionMultiplexer)))
        {
            throw new TransportInitializationException(
                "Redis transport requires IConnectionMultiplexer to be registered before AddRedisMessageTransport().");
        }

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton(typeof(IMessageQueueTransport<>), typeof(RedisMessageQueueTransport<>)));

        return services;
    }
}