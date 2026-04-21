using System.Text;
using System.Text.Json;
using Azure.Storage.Queues;
using DGC.Sample.Application.Common.Queue;
using DGC.Sample.Application.Dtos.Queue;
using DGC.Sample.Application.Interfaces.Queue;
using DGC.Sample.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DGC.Sample.Infrastructure.Queue;

public sealed class AzureStorageMessageQueueTransport<T> : IMessageQueueTransport<T>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _storageConnection;
    private readonly string _defaultQueueName;
    private readonly Dictionary<string, QueueClient> _queueClients = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _syncLock = new();
    private readonly int _maxPayloadBytes;

    public AzureStorageMessageQueueTransport(IConfiguration configuration)
    {
        var storageConnection = configuration["AzureWebJobsStorage"]
            ?? configuration.GetConnectionString("AzureWebJobsStorage");

        if (string.IsNullOrWhiteSpace(storageConnection))
        {
            throw new InternalErrorException(
                InternalErrorCode.QueueTransportInitializationError,
                "Azure queue transport requires 'AzureWebJobsStorage' configuration.",
                azureErrorDetails: null);
        }

        var queueName = configuration["Queue:Azure:QueueName"]
            ?? configuration["AzureFunctions:QueueName"];

        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new InternalErrorException(
                InternalErrorCode.QueueTransportInitializationError,
                "Azure queue transport requires queue name configuration at 'Queue:Azure:QueueName' or 'AzureFunctions:QueueName'.",
                azureErrorDetails: null);
        }

        _storageConnection = storageConnection;
        _defaultQueueName = queueName.Trim();

        ValidateQueueName(_defaultQueueName);
        _maxPayloadBytes = Math.Max(1024, configuration.GetValue<int?>("Queue:MaxPayloadBytes") ?? 262_144);
    }

    public QueueTransport TransportType => QueueTransport.AzureQueue;

    public async Task EnqueueAsync(T item, CancellationToken token)
    {
        await EnqueueAsync(item, queueName: null, token).ConfigureAwait(false);
    }

    public async Task EnqueueAsync(T item, string? queueName, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var resolvedQueueName = string.IsNullOrWhiteSpace(queueName)
            ? _defaultQueueName
            : queueName.Trim();

        ValidateQueueName(resolvedQueueName);
        var queueClient = GetQueueClient(resolvedQueueName);

        var envelope = CreateEnvelope(item);
        var payload = JsonSerializer.Serialize(envelope, JsonOptions);
        var payloadSize = Encoding.UTF8.GetByteCount(payload);

        if (payloadSize > _maxPayloadBytes)
        {
            throw new InternalErrorException(
                InternalErrorCode.QueueProcessingError,
                $"Queue payload exceeds max allowed bytes ({_maxPayloadBytes}).",
                innerCode: "payload_too_large",
                innerMessage: "Queue payload size is larger than configured maximum.");
        }

        await queueClient.CreateIfNotExistsAsync(cancellationToken: token).ConfigureAwait(false);
        await queueClient.SendMessageAsync(payload, cancellationToken: token).ConfigureAwait(false);
    }

    public Task<Envelope<T>?> DequeueAsync(int waitMs, CancellationToken token)
    {
        return DequeueAsync(waitMs, queueName: null, token);
    }

    public Task<Envelope<T>?> DequeueAsync(int waitMs, string? queueName, CancellationToken token)
    {
        throw new NotSupportedException(
            "Azure queue dequeue is not supported through IQueueService. Use Azure Functions queue trigger consumption.");
    }

    public Task AcknowledgeAsync(string envelopeId, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    public Task HandleProcessingErrorAsync(
        Envelope<T> envelope,
        int retryLimit,
        int retryDelayMs,
        ILogger logger,
        Exception exception,
        CancellationToken token)
    {
        throw new NotSupportedException(
            "Azure queue processing errors are handled by Azure Functions runtime retry/poison queue behavior.");
    }

    private static Envelope<T> CreateEnvelope(T item)
    {
        return new Envelope<T>(
            id: Guid.NewGuid().ToString("N"),
            payload: item,
            deliveryCount: 0,
            enqueuedAt: DateTimeOffset.UtcNow,
            typeName: typeof(T).FullName ?? typeof(T).Name,
            schemaVersion: 1);
    }

    private QueueClient GetQueueClient(string queueName)
    {
        lock (_syncLock)
        {
            if (_queueClients.TryGetValue(queueName, out var queueClient))
            {
                return queueClient;
            }

            var created = new QueueClient(_storageConnection, queueName);
            _queueClients[queueName] = created;
            return created;
        }
    }

    private static void ValidateQueueName(string queueName)
    {
        if (queueName.Length is < 3 or > 63)
        {
            throw new InternalErrorException(
                InternalErrorCode.QueueTransportInitializationError,
                "Azure queue name must be between 3 and 63 characters.",
                azureErrorDetails: null);
        }

        if (!char.IsLetterOrDigit(queueName[0]) || !char.IsLetterOrDigit(queueName[^1]))
        {
            throw new InternalErrorException(
                InternalErrorCode.QueueTransportInitializationError,
                "Azure queue name must start and end with a lowercase letter or number.",
                azureErrorDetails: null);
        }

        if (queueName.Any(c => !(char.IsLower(c) || char.IsDigit(c) || c == '-')))
        {
            throw new InternalErrorException(
                InternalErrorCode.QueueTransportInitializationError,
                "Azure queue name can contain only lowercase letters, digits, and '-'.",
                azureErrorDetails: null);
        }

        if (queueName.Contains("--", StringComparison.Ordinal))
        {
            throw new InternalErrorException(
                InternalErrorCode.QueueTransportInitializationError,
                "Azure queue name cannot contain consecutive hyphens.",
                azureErrorDetails: null);
        }
    }
}