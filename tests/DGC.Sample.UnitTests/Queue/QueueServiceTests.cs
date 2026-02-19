using DGC.Sample.Application.Queue;
using DGC.Sample.Infrastructure.Queue;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DGC.Sample.UnitTests.Queue;

public sealed class QueueServiceTests
{
    [Fact]
    public async Task EnqueueAndDequeue_ShouldUseConfiguredTransport()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new QueueServiceOptions { DefaultTransport = QueueTransport.InMemory });
        services.AddSingleton<IMessageQueueTransport<TestMessage>, InMemoryMessageQueueTransport<TestMessage>>();
        services.AddSingleton<ITransportResolver<TestMessage>, TransportResolver<TestMessage>>();
        services.AddSingleton<IQueueService, QueueService>();

        await using var provider = services.BuildServiceProvider();
        var queueService = provider.GetRequiredService<IQueueService>();

        await queueService.EnqueueAsync(new TestMessage("queued"), QueueTransport.InMemory, default);
        var dequeued = await queueService.DequeueAsync<TestMessage>(QueueTransport.InMemory, default);

        dequeued.Should().NotBeNull();
        dequeued!.Value.Should().Be("queued");
    }

    private sealed class TestMessage
    {
        public TestMessage(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}