using DGC.Sample.Application.Common.Queue;
using DGC.Sample.Application.Dtos.Queue;
using DGC.Sample.Application.Interfaces.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace DGC.Sample.Application.Services.Queue.Workers;

public abstract class MessageProcessingServiceBase<T>(
    IServiceScopeFactory scopeFactory,
    ITransportResolver<T> transportResolver,
    ILogger<MessageProcessingServiceBase<T>> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ITransportResolver<T> _transportResolver = transportResolver;
    private readonly ILogger _logger = logger;

    protected abstract string WorkerName { get; }

    protected abstract QueueTransport Transport { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var setupScope = _scopeFactory.CreateScope();
        var startupConfiguration = setupScope.ServiceProvider.GetRequiredService<IConfiguration>();

        var section = startupConfiguration.GetSection($"WorkerQueueSettings:{WorkerName}");
        var pollIntervalSeconds = Math.Max(1, section.GetValue<int?>("PollIntervalSeconds") ?? 1);
        var maxParallelism = Math.Max(1, section.GetValue<int?>("MaxDegreeOfParallelism") ?? 1);
        var queueName = section.GetValue<string>("QueueName");

        using var semaphore = new SemaphoreSlim(maxParallelism, maxParallelism);
        var inFlight = new HashSet<Task>();
        var transport = _transportResolver.Resolve(Transport);

        _logger.LogInformation(
            "Worker {WorkerName} started with transport={Transport} queueName={QueueName} pollInterval={PollIntervalSeconds}s maxParallelism={MaxParallelism}",
            WorkerName,
            Transport,
            string.IsNullOrWhiteSpace(queueName) ? "(default)" : queueName,
            pollIntervalSeconds,
            maxParallelism);

        while (!stoppingToken.IsCancellationRequested)
        {
            Envelope<T>? envelope;

            using (var scope = _scopeFactory.CreateScope())
            {
                envelope = await transport.DequeueAsync(pollIntervalSeconds * 1000, queueName, stoppingToken).ConfigureAwait(false);
            }

            if (envelope is null)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken).ConfigureAwait(false);
                continue;
            }

            await semaphore.WaitAsync(stoppingToken).ConfigureAwait(false);

            var processingTask = ProcessEnvelopeAsync(envelope, transport, semaphore, stoppingToken);

            lock (inFlight)
            {
                inFlight.Add(processingTask);
            }

            _ = processingTask.ContinueWith(_ =>
            {
                lock (inFlight)
                {
                    inFlight.Remove(processingTask);
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        Task[] remaining;
        lock (inFlight)
        {
            remaining = inFlight.ToArray();
        }

        if (remaining.Length > 0)
        {
            await Task.WhenAll(remaining).ConfigureAwait(false);
        }
    }

    private async Task ProcessEnvelopeAsync(
        Envelope<T> envelope,
        IMessageQueueTransport<T> transport,
        SemaphoreSlim semaphore,
        CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var retryLimit = configuration.GetValue<int?>("Queue:Retry:Limit") ?? 10;
            var retryDelayMs = configuration.GetValue<int?>("Queue:Retry:DelayMs") ?? 100;

            try
            {
                await ProcessMessageAsync(scope.ServiceProvider, envelope.Payload, stoppingToken).ConfigureAwait(false);
                await transport.AcknowledgeAsync(envelope.Id, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await transport.HandleProcessingErrorAsync(envelope, retryLimit, retryDelayMs, _logger, ex, stoppingToken).ConfigureAwait(false);
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    protected abstract Task ProcessMessageAsync(IServiceProvider serviceProvider, T message, CancellationToken token);
}