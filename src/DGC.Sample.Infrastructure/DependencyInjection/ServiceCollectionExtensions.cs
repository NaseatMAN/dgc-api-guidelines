using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Interfaces.Notifications;
using DGC.Sample.Application.Queue;
using DGC.Sample.Infrastructure.ExternalServices.Notifications;
using DGC.Sample.Application.Queue.Exceptions;
using DGC.Sample.Infrastructure.Caching;
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

        // Redis & Caching
        services.AddRedisServices(configuration);

        // Caching & Idempotency
        services.AddHybridCache();
        services.AddScoped<IIdempotencyService, HybridCacheIdempotencyService>();

        // Redis & Caching
        services.AddRedisServices(configuration);

        // Caching & Idempotency
        services.AddHybridCache();
        services.AddScoped<IIdempotencyService, HybridCacheIdempotencyService>();
        //services.AddScoped<IIdempotencyService, IdempotencyService>();
        services.AddNotificationServices(configuration);

        services.AddQueueServices(configuration);

        return services;
    }

    private static IServiceCollection AddRedisServices(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(redisConnection))
        {
            return services;
        }

        // Register ConnectionMultiplexer for custom Redis usage (like the Queue)
        services.TryAddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));

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

    public static IServiceCollection AddNotificationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var emailSettings = configuration
            .GetSection(EmailNotificationSettings.SectionName)
            .Get<EmailNotificationSettings>()
            ?? new EmailNotificationSettings();

        var telegramSettings = configuration
            .GetSection(TelegramNotificationSettings.SectionName)
            .Get<TelegramNotificationSettings>()
            ?? new TelegramNotificationSettings();

        services.TryAddSingleton(emailSettings);
        services.TryAddSingleton(telegramSettings);
        services.TryAddScoped<IEmailSender, SmtpEmailSender>();
        services.TryAddScoped<ITelegramSender, TelegramSender>();

        return services;
    }
}
