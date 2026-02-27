using DGC.Sample.Application.Common.Queue;
using DGC.Sample.Application.Dtos.Queue;
using DGC.Sample.Application.Interfaces.Queue;
using DGC.Sample.Domain.Exceptions;
using DGC.Sample.Infrastructure.Queue;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DGC.Sample.UnitTests.Queue;

public sealed class TransportResolverTests
{
    [Fact]
    public void Resolve_WhenTransportMissing_ShouldThrowInternalErrorException()
    {
        var inMemoryTransport = new StubTransport<string>(QueueTransport.InMemory);
        var resolver = new TransportResolver<string>(new[] { inMemoryTransport });

        var act = () => resolver.Resolve(QueueTransport.Redis);

        var exception = act.Should().Throw<InternalErrorException>().Which;
        exception.ResponseBody.Error.Code.Should().Be(InternalErrorCode.QueueTransportNotRegistered);
    }

    [Fact]
    public void Ctor_WhenDuplicateTransportsRegistered_ShouldThrowInternalErrorException()
    {
        var one = new StubTransport<string>(QueueTransport.InMemory);
        var two = new StubTransport<string>(QueueTransport.InMemory);

        var act = () => new TransportResolver<string>(new IMessageQueueTransport<string>[] { one, two });

        var exception = act.Should().Throw<InternalErrorException>().Which;
        exception.ResponseBody.Error.Code.Should().Be(InternalErrorCode.QueueTransportInitializationError);
    }

    private sealed class StubTransport<T>(QueueTransport transportType) : IMessageQueueTransport<T>
    {
        public QueueTransport TransportType { get; } = transportType;

        public Task EnqueueAsync(T item, CancellationToken token = default) => Task.CompletedTask;

        public Task EnqueueAsync(T item, string? queueName, CancellationToken token = default) => Task.CompletedTask;

        public Task<Envelope<T>?> DequeueAsync(int waitMs, CancellationToken token = default)
            => Task.FromResult<Envelope<T>?>(null);

        public Task<Envelope<T>?> DequeueAsync(int waitMs, string? queueName, CancellationToken token = default)
            => Task.FromResult<Envelope<T>?>(null);

        public Task AcknowledgeAsync(string envelopeId, CancellationToken token = default) => Task.CompletedTask;

        public Task HandleProcessingErrorAsync(
            Envelope<T> envelope,
            int retryLimit,
            int retryDelayMs,
            ILogger logger,
            Exception exception,
            CancellationToken token = default)
            => Task.CompletedTask;
    }
}