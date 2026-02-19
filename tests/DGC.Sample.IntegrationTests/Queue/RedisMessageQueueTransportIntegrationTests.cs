using DGC.Sample.Infrastructure.Queue;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using Xunit;

namespace DGC.Sample.IntegrationTests.Queue;

public sealed class RedisMessageQueueTransportIntegrationTests : IAsyncLifetime
{
    private IConnectionMultiplexer? _multiplexer;
    private IDatabase? _database;
    private bool _redisAvailable;

    public async Task InitializeAsync()
    {
        try
        {
            _multiplexer = await ConnectionMultiplexer.ConnectAsync("localhost:6379,abortConnect=false");
            _database = _multiplexer.GetDatabase();
            await _database.ExecuteAsync("FLUSHDB");
            _redisAvailable = true;
        }
        catch
        {
            _redisAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (_redisAvailable && _database is not null)
        {
            await _database.ExecuteAsync("FLUSHDB");
        }

        _multiplexer?.Dispose();
    }

    [Fact]
    public async Task EnqueueDequeueAcknowledge_ShouldRoundTripMessage()
    {
        if (!_redisAvailable)
        {
            return;
        }

        var transport = CreateTransport();
        var message = new RedisTestMessage("integration");

        await transport.EnqueueAsync(message, default);
        var envelope = await transport.DequeueAsync(waitMs: 50, token: default);

        envelope.Should().NotBeNull();
        envelope!.Payload.Value.Should().Be("integration");

        await transport.AcknowledgeAsync(envelope.Id, default);

        var processingListLength = await _database!.ListLengthAsync("queue:redistestmessage:processing");
        processingListLength.Should().Be(0);
    }

    [Fact]
    public async Task HandleProcessingError_WhenRetryExceeded_ShouldMoveToDeadLetterQueue()
    {
        if (!_redisAvailable)
        {
            return;
        }

        var transport = CreateTransport();
        await transport.EnqueueAsync(new RedisTestMessage("to-dlq"), default);

        var envelope = await transport.DequeueAsync(waitMs: 50, token: default);
        envelope.Should().NotBeNull();

        await transport.HandleProcessingErrorAsync(
            envelope!,
            retryLimit: 0,
            retryDelayMs: 0,
            logger: NullLogger.Instance,
            exception: new InvalidOperationException("boom"),
            token: default);

        var dlqLength = await _database!.ListLengthAsync("deadletter:queue:redistestmessage");
        dlqLength.Should().Be(1);
    }

    private RedisMessageQueueTransport<RedisTestMessage> CreateTransport()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Queue:DeadLetter:Enabled"] = "true",
                ["Queue:DeadLetter:Prefix"] = "deadletter",
                ["Queue:MaxPayloadBytes"] = "262144"
            })
            .Build();

        return new RedisMessageQueueTransport<RedisTestMessage>(_multiplexer!, config);
    }

    private sealed class RedisTestMessage
    {
        public RedisTestMessage()
        {
        }

        public RedisTestMessage(string value)
        {
            Value = value;
        }

        public string Value { get; set; } = string.Empty;
    }
}