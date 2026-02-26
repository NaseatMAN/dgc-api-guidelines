using DGC.Sample.Application.Queue;
using DGC.Sample.Application.Queue.Exceptions;
using DGC.Sample.Application.Queue.Messages;
using DGC.Sample.Application.Queue.Workers;
using DGC.Sample.Application.Queue.Workers.Handlers;
using DGC.Sample.Infrastructure.Queue;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace DGC.Sample.Api.Extensions;

public static class QueueExtensions
{
    public static IServiceCollection AddQueueInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(configuration);

        var defaultTransportRaw = configuration["Queue:DefaultTransport"];
        var defaultTransport = Enum.TryParse<QueueTransport>(defaultTransportRaw, true, out var parsedTransport)
            ? parsedTransport
            : QueueTransport.InMemory;

        var azureStorageConnection = configuration["AzureWebJobsStorage"]
            ?? configuration.GetConnectionString("AzureWebJobsStorage");
        var azureQueueName = configuration["Queue:Azure:QueueName"]
            ?? configuration["AzureFunctions:QueueName"];

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
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton(typeof(IMessageQueueTransport<>), typeof(RedisMessageQueueTransport<>)));
        }

        if (!string.IsNullOrWhiteSpace(azureStorageConnection) && !string.IsNullOrWhiteSpace(azureQueueName))
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton(typeof(IMessageQueueTransport<>), typeof(AzureStorageMessageQueueTransport<>)));
        }
        else if (defaultTransport == QueueTransport.AzureQueue)
        {
            throw new TransportInitializationException(
                "Queue:DefaultTransport is set to AzureQueue, but required configuration is missing. Set 'AzureWebJobsStorage' and 'Queue:Azure:QueueName' (or 'AzureFunctions:QueueName').");
        }

        services.TryAddSingleton(typeof(ITransportResolver<>), typeof(TransportResolver<>));
        services.TryAddSingleton<IQueueService, QueueService>();

        return services;
    }

    public static IServiceCollection AddApiQueueWorkers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMessageHandler<OrderCreatedMessage>, OrderCreatedMessageHandler>();
        services.AddHostedService<BackgroundOrderCreatedWorker>();

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddHostedService<BackgroundOrderCreatedRedisWorker>();
        }

        return services;
    }
}
