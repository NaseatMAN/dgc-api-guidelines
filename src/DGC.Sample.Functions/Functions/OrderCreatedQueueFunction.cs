using System.Text.Json;
using DGC.Sample.Application.Dtos.Queue;
using DGC.Sample.Application.Interfaces.Queue;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DGC.Sample.Functions.Functions;

public sealed class OrderCreatedQueueFunction(
    IMessageHandler<OrderCreatedMessage> messageHandler,
    ILogger<OrderCreatedQueueFunction> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly IMessageHandler<OrderCreatedMessage> _messageHandler = messageHandler;
    private readonly ILogger<OrderCreatedQueueFunction> _logger = logger;

    [Function("OrderCreatedQueueConsumer")]
    public async Task RunAsync(
        [QueueTrigger("%AzureFunctions:QueueName%", Connection = "AzureWebJobsStorage")] string queueMessage,
        CancellationToken cancellationToken)
    {
        var envelope = JsonSerializer.Deserialize<Envelope<OrderCreatedMessage>>(queueMessage, JsonOptions);
        if (envelope?.Payload is null)
        {
            throw new InvalidOperationException("Queue payload must be a valid Envelope<OrderCreatedMessage> with a non-null payload.");
        }

        await _messageHandler.HandleAsync(envelope.Payload, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Processed queue trigger message. function=OrderCreatedQueueConsumer envelopeId={EnvelopeId} orderId={OrderId}",
            envelope.Id,
            envelope.Payload.OrderId);
    }
}