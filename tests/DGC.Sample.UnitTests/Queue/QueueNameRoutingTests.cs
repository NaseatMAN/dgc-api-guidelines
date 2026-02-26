using DGC.Sample.Application.Queue;
using DGC.Sample.Application.Queue.Exceptions;
using DGC.Sample.Infrastructure.Queue;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DGC.Sample.UnitTests.Queue;

public sealed class QueueNameRoutingTests
{
    [Fact]
    public async Task EnqueueAsync_WithQueueName_ShouldForwardQueueNameToTransport()
    {
        var transport = new CaptureTransport<TestMessage>(QueueTransport.AzureQueue);

        var services = new ServiceCollection();
        services.AddSingleton(new QueueServiceOptions { DefaultTransport = QueueTransport.InMemory });
        services.AddSingleton<IMessageQueueTransport<TestMessage>>(transport);
        services.AddSingleton<ITransportResolver<TestMessage>, TransportResolver<TestMessage>>();
        services.AddSingleton<IQueueService, QueueService>();

        await using var provider = services.BuildServiceProvider();
        var queueService = provider.GetRequiredService<IQueueService>();

        await queueService.EnqueueAsync(new TestMessage("queued"), QueueTransport.AzureQueue, "orders-priority", CancellationToken.None);

        transport.LastQueueName.Should().Be("orders-priority");
    }

    [Fact]
    public async Task AzureTransport_EnqueueAsync_WithInvalidQueueName_ShouldThrow()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureWebJobsStorage"] = "UseDevelopmentStorage=true",
                ["AzureFunctions:QueueName"] = "orders"
            })
            .Build();

        var transport = new AzureStorageMessageQueueTransport<TestMessage>(configuration);

        var act = () => transport.EnqueueAsync(new TestMessage("queued"), "Orders_Upper", CancellationToken.None);

        await act.Should().ThrowAsync<TransportInitializationException>();
    }

    private sealed record TestMessage(string Value);

    private sealed class CaptureTransport<T>(QueueTransport transportType) : IMessageQueueTransport<T>
    {
        public QueueTransport TransportType { get; } = transportType;

        public string? LastQueueName { get; private set; }

        public Task EnqueueAsync(T item, CancellationToken token = default)
        {
            LastQueueName = null;
            return Task.CompletedTask;
        }

        public Task EnqueueAsync(T item, string? queueName, CancellationToken token = default)
        {
            LastQueueName = queueName;
            return Task.CompletedTask;
        }

        public Task<Envelope<T>?> DequeueAsync(int waitMs, CancellationToken token = default)
            => Task.FromResult<Envelope<T>?>(null);

        public Task<Envelope<T>?> DequeueAsync(int waitMs, string? queueName, CancellationToken token = default)
            => Task.FromResult<Envelope<T>?>(null);

        public Task AcknowledgeAsync(string envelopeId, CancellationToken token = default)
            => Task.CompletedTask;

        public Task HandleProcessingErrorAsync(Envelope<T> envelope, int retryLimit, int retryDelayMs, ILogger logger, Exception exception, CancellationToken token = default)
            => Task.CompletedTask;
    }
}