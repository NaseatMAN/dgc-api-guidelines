using DGC.Sample.Infrastructure.Queue;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DGC.Sample.UnitTests.Queue;

public sealed class InMemoryMessageQueueTransportTests
{
    [Fact]
    public async Task DequeueAsync_WhenQueueEmpty_ShouldReturnNull()
    {
        var transport = new InMemoryMessageQueueTransport<TestMessage>();

        var result = await transport.DequeueAsync(waitMs: 5, token: default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task EnqueueThenDequeue_ShouldReturnMessage()
    {
        var transport = new InMemoryMessageQueueTransport<TestMessage>();
        var payload = new TestMessage("hello");

        await transport.EnqueueAsync(payload, CancellationToken.None);
        var dequeued = await transport.DequeueAsync(waitMs: 0, token: default);

        dequeued.Should().NotBeNull();
        dequeued!.Payload.Value.Should().Be("hello");
    }

    [Fact]
    public async Task HandleProcessingErrorAsync_WhenRetryExceeded_ShouldMoveToDeadLetterQueue()
    {
        var transport = new InMemoryMessageQueueTransport<TestMessage>();
        await transport.EnqueueAsync(new TestMessage("retry"), CancellationToken.None);

        var dequeued = await transport.DequeueAsync(waitMs: 0, token: default);
        dequeued.Should().NotBeNull();

        await transport.HandleProcessingErrorAsync(
            dequeued!,
            retryLimit: 0,
            retryDelayMs: 0,
            logger: NullLogger.Instance,
            exception: new InvalidOperationException("boom"),
            token: default);

        transport.DeadLetterCount.Should().Be(1);
    }

    [Fact]
    public async Task EnqueueAsync_WithQueueName_ShouldIsolateFromDefaultQueue()
    {
        var transport = new InMemoryMessageQueueTransport<TestMessage>();

        await transport.EnqueueAsync(new TestMessage("named"), "priority", CancellationToken.None);

        var dequeued = await transport.DequeueAsync(waitMs: 0, token: default);

        dequeued.Should().BeNull();
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