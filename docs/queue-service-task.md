---
# QueueService + Hosted Dequeue Worker — Task Plan (NLP-friendly)

## Intent

Provide clear, actionable planning for adding a generic, transport-agnostic `IQueueService` and a hosted dequeue worker to the project. This document is written to be easily parsed by humans and NLP tools (short metadata, explicit tasks, clear acceptance criteria).

---

## Metadata

- intent: implement-queue-service
- transports: InMemory | Redis (optional)
- redis-client: StackExchange.Redis (when used)
- api-style: async-only, nullable-dequeue
- code-conventions: file-scoped namespaces; primary constructors for hosted worker
 - worker-config-key: `WorkerQueueSettings:{workerName}:MaxDegreeOfParallelism` (int)

---

## Overview

The goal is to expose an `IQueueService` producers can call to enqueue typed messages, and consumers (hosted workers) can dequeue and process them. The project should include both an in-memory implementation and an optional Redis-backed implementation. Transport selection is DI-driven: registering Redis types enables Redis; otherwise in-memory is available.

---

## Key Decisions (short)

- All public API methods must be async.
- `DequeueAsync<T>` returns a nullable `T?` (null = no item available).
- Transport selection is determined by DI registration (no config key to switch transports at runtime).
- Use StackExchange.Redis for Redis implementation if the app wires it into DI.
- Use file-scoped namespaces and primary constructors where applicable (hosted worker example uses a primary constructor).
- DTOs for queue messages must provide a parameterless constructor and a full-field constructor.

---

## Functional Requirements

- Thread-safe enqueue/dequeue for arbitrary `T`.
- Producers can call `EnqueueAsync<T>(T item)` without needing transport-specific knowledge.
- Consumers/hosted workers call `DequeueAsync<T>()` and handle `null` as "no item".
- Hosted worker polls the queue, processes items, handles exceptions, and respects `CancellationToken`.

---

## API Proposal (concise)

Transport enum (proposal):

```csharp
public enum QueueTransport
{
  InMemory,
  Redis,
  AzureQueue,
  Hangfire
}
```

Interface (async-only) with transport flag (default InMemory):

```csharp
public interface IQueueService
{
  /// <summary>
  /// Enqueue an item. The default transport is <see cref="QueueTransport.InMemory"/>.
  /// If an explicit transport is requested but that transport has not been registered in DI,
  /// the call must fail fast with a clear exception.
  /// </summary>
  Task EnqueueAsync<T>(T item, QueueTransport transport = QueueTransport.InMemory, CancellationToken cancellationToken = default);

  /// <summary>
  /// Attempts to dequeue an item of type T. Returns the item when available, or `null` when no item is
  /// currently available. The default transport is <see cref="QueueTransport.InMemory"/>.
  /// If an explicit transport is requested but not registered, the call must fail fast.
  /// </summary>
  Task<T?> DequeueAsync<T>(QueueTransport transport = QueueTransport.InMemory, CancellationToken cancellationToken = default);
}
```

Notes: `DequeueAsync<T>` returning `null` signals "no item" not an error. The optional `transport` flag allows callers to request a specific transport; if the requested transport is unavailable (not registered in DI), the implementation must throw a descriptive exception rather than silently falling back to in-memory.

---

## Hosted Worker (primary-constructor example)

```csharp
// Message-processing background service: scope-per-message, retry and dead-letter delegated to the transport
public abstract class MessageProcessingServiceBase<T>(IServiceScopeFactory scopeFactory, ILogger<MessageProcessingServiceBase<T>> logger) : BackgroundService
{
  private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
  private readonly ILogger _logger = logger;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      using var scope = _scopeFactory.CreateScope();
      var transport = scope.ServiceProvider.GetRequiredService<IMessageQueueTransport<T>>();
      var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

      var retryLimit = configuration.GetValue<int>("Queue:Retry:Limit", 10);
      var retryDelayMs = configuration.GetValue<int>("Queue:Retry:DelayMs", 100);

      var envelope = await transport.DequeueAsync(retryDelayMs, stoppingToken);
      if (envelope is null)
      {
        // short delay to avoid a tight loop when queue is empty
        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken).ContinueWith(_ => { });
        continue;
      }

      try
      {
        await ProcessMessageAsync(scope.ServiceProvider, envelope.Payload, stoppingToken).ConfigureAwait(false);
        await transport.AcknowledgeAsync(envelope.Id, stoppingToken).ConfigureAwait(false);
      }
      catch (OperationCanceledException)
      {
        throw; // let host shut down
      }
      catch (Exception ex)
      {
        // delegate retry/backoff/dead-letter handling to the transport implementation
        await transport.HandleProcessingErrorAsync(envelope, retryLimit, retryDelayMs, _logger, ex, stoppingToken).ConfigureAwait(false);
      }
    }
  }

  protected abstract Task ProcessMessageAsync(IServiceProvider serviceProvider, T message, CancellationToken token);
}

// Envelope and queue contract samples
public record Envelope<T>(string Id, T Payload, int DeliveryCount, DateTimeOffset EnqueuedAt);

public interface IMessageQueueTransport<T>
{
  Task<Envelope<T>?> DequeueAsync(int waitMs, CancellationToken token);
  Task AcknowledgeAsync(string envelopeId, CancellationToken token);
  Task HandleProcessingErrorAsync(Envelope<T> envelope, int retryLimit, int retryDelayMs, ILogger logger, Exception exception, CancellationToken token);
}
```

Notes:
- The `MessageProcessingServiceBase<T>` encapsulates the scope-per-message pattern, acknowledges successful processing, and delegates retry/DLQ logic to `IMessageQueueTransport<T>`.
- `DequeueAsync` returns an `Envelope<T>` or `null` when empty. `HandleProcessingErrorAsync` should implement retries, backoff and DLQ moves.
- Reading retry config per-scope allows live reload if needed; move to cached/central config if performance is a concern.

---

## DI Rules (explicit)

- There is no feature flag or configuration key to switch transports.
- Registering a transport implementation in DI (for example, a Redis-backed `IQueueService`) makes that transport available for explicit use when callers request it via the `QueueTransport` flag. The default transport remains `InMemory`.
- If the consumer does not register any additional transports, the project should rely on the in-memory queue.
- If transport registration is attempted but required dependencies (e.g., `IConnectionMultiplexer` for Redis) are missing, registration must fail fast with a clear startup exception.

---

## Multiple Specialized Workers

You can run multiple worker instances in the same application (for example: `BackgroundUpdateWorker`, `BackgroundLogWorker`, `ReportingWorker`). Each worker is configured independently and uses its own message type, handler, and concurrency limit.

Configuration (example):

```yaml
WorkerQueueSettings:
  BackgroundUpdate:
    PollIntervalSeconds: 5
    MaxDegreeOfParallelism: 4
  BackgroundLog:
    PollIntervalSeconds: 1
    MaxDegreeOfParallelism: 2
```

DI registration example (in `Program.cs` / `Startup`):

```csharp
// register default/in-memory transport implementation
services.AddInMemoryMessageTransport();

// register any business services used by handlers
services.AddSingleton<IUpdateService, UpdateService>();
services.AddSingleton<ILogService, LogService>();

// BackgroundUpdate worker (DI constructs the worker)
services.AddHostedService<BackgroundUpdateWorker>();

// BackgroundLog worker (DI constructs the worker)
services.AddHostedService<BackgroundLogWorker>();

// --- Concrete worker example (primary-constructor style) - BackgroundUpdateWorker
```csharp
// A simple handler interface implementers should register as scoped.
public interface IMessageHandler<T>
{
  Task HandleAsync(T message, CancellationToken token);
}

// A concrete worker for `UpdateMessage`. It uses the typed queue and handler for the message.
public sealed class BackgroundUpdateWorker(IServiceProvider provider, IMessageQueueTransport<UpdateMessage> transport, ILogger<BackgroundUpdateWorker> logger) : BackgroundService
{
  private readonly IServiceProvider _provider = provider;
  private readonly IMessageQueueTransport<UpdateMessage> _transport = transport;
  private readonly ILogger _logger = logger;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    const string workerName = "BackgroundUpdate";
    var configuration = _provider.GetRequiredService<IConfiguration>();

    var section = configuration.GetSection($"WorkerQueueSettings:{workerName}");
    var pollIntervalSeconds = section.GetValue<int?>("PollIntervalSeconds") ?? 1;
    var maxParallel = Math.Max(1, section.GetValue<int?>("MaxDegreeOfParallelism") ?? 1);

    using var semaphore = new SemaphoreSlim(maxParallel);

    _logger.LogInformation("{Worker} starting with pollInterval={Poll}s maxParallel={Max}", workerName, pollIntervalSeconds, maxParallel);

    while (!stoppingToken.IsCancellationRequested)
    {
      var envelope = await _transport.DequeueAsync(pollIntervalSeconds * 1000, stoppingToken).ConfigureAwait(false);
      if (envelope is null)
      {
        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken).ContinueWith(_ => { });
        continue;
      }

      await semaphore.WaitAsync(stoppingToken).ConfigureAwait(false);

      _ = Task.Run(async () =>
      {
        using var scope = _provider.CreateScope();
            try
            {
              var handler = scope.ServiceProvider.GetService<IMessageHandler<UpdateMessage>>();
              if (handler is null)
              {
                _logger.LogWarning("No IMessageHandler<UpdateMessage> registered for worker {Worker}", workerName);
                await _transport.HandleProcessingErrorAsync(envelope, retryLimit: 0, retryDelayMs: 0, _logger, new InvalidOperationException("No handler"), stoppingToken).ConfigureAwait(false);
                return;
              }

              await handler.HandleAsync(envelope.Payload, stoppingToken).ConfigureAwait(false);
              await _transport.AcknowledgeAsync(envelope.Id, stoppingToken).ConfigureAwait(false);
            }
        catch (OperationCanceledException)
        {
          // let host shut down
        }
          catch (Exception ex)
          {
            var retryLimit = configuration.GetValue<int>("Queue:Retry:Limit", 10);
            var retryDelayMs = configuration.GetValue<int>("Queue:Retry:DelayMs", 100);
            await _transport.HandleProcessingErrorAsync(envelope, retryLimit, retryDelayMs, _logger, ex, stoppingToken).ConfigureAwait(false);
          }
        finally
        {
          semaphore.Release();
        }
      }, stoppingToken);
    }
  }
}
```

// Usage: register a scoped handler and the hosted worker
```csharp
services.AddScoped<IMessageHandler<UpdateMessage>, UpdateMessageHandler>();
services.AddHostedService<BackgroundUpdateWorker>();
```
```

Notes and recommendations:

- Each worker reads its own `MaxDegreeOfParallelism` from `WorkerQueueSettings:{WorkerName}:MaxDegreeOfParallelism` and creates its `SemaphoreSlim` accordingly.
- Handlers should be short-lived, cancelable, and idempotent when possible.
- Prefer defining a small helper/extension `AddQueueWorker<TMessage>(this IServiceCollection services, string workerName, Func<IServiceProvider, Func<TMessage, CancellationToken, Task>> handlerFactory)` to reduce boilerplate.

### Explicit DI registrations (example)

Use these snippets to register open-generic queue implementations, handlers, and the hosted workers.

```csharp
// --- In-memory transport (open-generic registration)
// registers `IMessageQueueTransport<T>` -> `InMemoryMessageQueueTransport<T>`
services.AddSingleton(typeof(IMessageQueueTransport<>), typeof(InMemoryMessageQueueTransport<>));
// or, if you provide a helper extension:
// services.AddInMemoryMessageTransport();

// Register business handlers (scoped)
services.AddScoped<IUpdateService, UpdateService>();
services.AddScoped<ILogService, LogService>();

// Register hosted workers (DI will construct concrete workers)
services.AddHostedService<BackgroundUpdateWorker>();
services.AddHostedService<BackgroundLogWorker>();

// --- Optional: Redis-backed transport (register connection and Redis transport impl)
// var mux = ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis"));
// services.AddSingleton<IConnectionMultiplexer>(mux);
// services.AddSingleton(typeof(IMessageQueueTransport<>), typeof(RedisMessageQueueTransport<>));
// or use a helper extension: services.AddRedisMessageTransport(mux);
```

Notes:
 - Registering a Redis-backed `IMessageQueueTransport<>` will replace the in-memory `IMessageQueueTransport<>` registration for DI resolution; explicit transport flags still control semantics at call-time.
- Handlers are typically registered as scoped so each message scope can resolve fresh instances.

---

---

## DTO Conventions

- Queue messages SHOULD have:
  - A public parameterless constructor for deserialization / empty initialization.
  - A public constructor that accepts all required fields for convenient creation.
  - Mutable or init-only properties as appropriate for serialization.

Example: `QueueMessage` should include both constructors.

---

## Tasks (actionable, ordered)

1. Draft `IQueueService` API (async-only, nullable dequeue). [owner: TBD]
2. Add a concise design doc section explaining DI-driven transport selection. [owner: TBD]
3. Add in-memory implementation notes and test plan. [owner: TBD]
4. Add Redis implementation notes (StackExchange.Redis) and guard rules for DI registration. [owner: TBD]
5. Define worker behavior and provide primary-constructor example (e.g. `BackgroundUpdateWorker`). [owner: TBD]
6. Add unit & integration test outlines (enqueue/dequeue, worker lifecycle). [owner: TBD]
7. Final acceptance tests and checklist. [owner: TBD]

Each task should be small and verifiable; use the checklist above as acceptance criteria for each step.

---

## Acceptance Criteria (NLP-friendly list)

- `IQueueService` async API is documented and approved.
- The hosted worker example demonstrates primary-constructor usage.
- DI-driven transport rule is unambiguous in the docs.
- DTO constructor convention is documented.
- Tests are defined for enqueue/dequeue and worker lifecycle (start, process, stop, cancellation).

---

## Notes for Implementer

- Keep implementations out of this planning doc; include only clear API shapes and DI rules.
- When implementing Redis, prefer `StackExchange.Redis` and JSON serialization with `System.Text.Json`.
- Avoid adding runtime switches or config keys to choose transport — let DI presence drive availability.

---

## Recommended Processing Model (message-processing pattern)

Adopt the pattern in the provided sample `MessageProcessingServiceBase<T>`: a scope-per-message BackgroundService that:

- Creates an `IServiceScope` for each iteration and resolves a typed queue from that scope.
- Dequeues an envelope (message + metadata) and delegates business work to an abstract `ProcessMessageAsync(IServiceProvider, T, CancellationToken)` or a scoped handler resolved per message.
- Delegates retry/backoff/poison/dead-letter logic to the queue abstraction where practical, or to a shared helper used by the worker.

This pattern guarantees scoped services are resolved per-message and disposed properly, keeps retry semantics consistent, and centralizes queue-specific concerns (visibility, requeueing) inside the queue implementation.

---

## Envelope model and queue responsibilities

- Envelope: messages should be transported inside an envelope that carries payload `T`, retry count/metadata, timestamps and an id.
- Queue responsibilities:
  - `DequeueAsync(...)` returns an envelope or null when empty.
  - `AcknowledgeAsync(envelope)` or implicit removal on successful processing.
  - `HandleProcessingErrorAsync(envelope, ...)` (or similar) to implement retry/back-off and dead-letter routing.
  - Support inspecting and moving messages to a dead-letter queue (DLQ) when retry limit exceeded.

Suggested transport interface, resolver and facade (conceptual):

```csharp
// transport identifier
public enum QueueTransport { InMemory, Redis, AzureQueue, Hangfire }

// message envelope
public record Envelope<T>(string Id, T Payload, int DeliveryCount, DateTimeOffset EnqueuedAt);

// per-transport implementation contract
public interface IMessageQueueTransport<T>
{
  QueueTransport TransportType { get; }
  Task EnqueueAsync(T item, CancellationToken token = default);
  Task<Envelope<T>?> DequeueAsync(int waitMs, CancellationToken token = default);
  Task AcknowledgeAsync(string envelopeId, CancellationToken token = default);
  Task HandleProcessingErrorAsync(Envelope<T> envelope, int retryLimit, int retryDelayMs, ILogger logger, Exception exception, CancellationToken token = default);
}

// resolver used by the public-facing facade to pick a transport implementation for T
public interface ITransportResolver<T>
{
  IMessageQueueTransport<T> Resolve(QueueTransport transport); // throws when not registered
  bool TryResolve(QueueTransport transport, out IMessageQueueTransport<T>? transportImpl);
}

public class TransportResolver<T> : ITransportResolver<T>
{
  private readonly Dictionary<QueueTransport, IMessageQueueTransport<T>> _map;
  public TransportResolver(IEnumerable<IMessageQueueTransport<T>> transports)
  {
    _map = transports.ToDictionary(t => t.TransportType);
  }
  public IMessageQueueTransport<T> Resolve(QueueTransport transport)
    => _map.TryGetValue(transport, out var t) ? t : throw new InvalidOperationException($"Transport {transport} not registered for {typeof(T).Name}");
  public bool TryResolve(QueueTransport transport, out IMessageQueueTransport<T>? transportImpl)
    => _map.TryGetValue(transport, out transportImpl);
}

// public facade used by application code
public interface IQueueService
{
  Task EnqueueAsync<T>(T item, QueueTransport transport = QueueTransport.InMemory, CancellationToken cancellationToken = default);
  Task<T?> DequeueAsync<T>(QueueTransport transport = QueueTransport.InMemory, CancellationToken cancellationToken = default);
}

public class QueueService : IQueueService
{
  private readonly IServiceProvider _provider;
  public QueueService(IServiceProvider provider) => _provider = provider;

  public async Task EnqueueAsync<T>(T item, QueueTransport transport = QueueTransport.InMemory, CancellationToken cancellationToken = default)
  {
    var resolver = _provider.GetRequiredService<ITransportResolver<T>>();
    var impl = resolver.Resolve(transport);
    await impl.EnqueueAsync(item, cancellationToken).ConfigureAwait(false);
  }

  public async Task<T?> DequeueAsync<T>(QueueTransport transport = QueueTransport.InMemory, CancellationToken cancellationToken = default)
  {
    var resolver = _provider.GetRequiredService<ITransportResolver<T>>();
    var impl = resolver.Resolve(transport);
    var env = await impl.DequeueAsync(0, cancellationToken).ConfigureAwait(false);
    return env?.Payload;
  }
}
```

---

## Retry, Backoff and Dead-letter

- Retry policy: use a configurable retry limit and delay/backoff strategy. Prefer exponential backoff with jitter for remote transports.
- Dead-letter: when DeliveryCount >= RetryLimit, move the envelope to a DLQ (separate key/queue) and log with details.
- Visibility / atomicity (Redis): for Redis use a reliable dequeue pattern (BRPOP + processing list, or RPOPLPUSH / BRPOPLPUSH pattern) so messages are not lost on crash.

Config keys (recommended):

 - `WorkerQueueSettings:{WorkerName}:MaxDegreeOfParallelism` (int)
 - `WorkerQueueSettings:{WorkerName}:PollIntervalSeconds` (int)
- `Queue:Retry:Limit` (int) — default 10
- `Queue:Retry:DelayMs` (int) — default 100
- `Queue:Retry:Backoff` — `fixed|exponential` (string)
- `Queue:DeadLetter:Enabled` (bool)
- `Queue:DeadLetter:Prefix` (string)

---

## Implementation Decisions (authoritative defaults)

These are the explicit defaults implementers should follow to avoid ambiguity.

- **Facade vs transport**: `IQueueService` is the public façade. `IMessageQueueTransport<T>` are per-transport implementations; transports own transport-specific concerns (visibility, atomic ops, DLQ).
- **Dequeue semantics**: `DequeueAsync(int waitMs, ...)` is a blocking wait up to `waitMs` milliseconds (0 = no-wait). Transports MAY expose a `DequeueAsync(TimeSpan visibility, ...)` overload to express lease semantics.
- **Acknowledgement & visibility**: For remote transports, dequeuing grants a lease/visibility window. `AcknowledgeAsync` removes the message; if the lease expires without ack the transport may make the message visible again.
- **DeliveryCount / retries**: The transport increments `Envelope.DeliveryCount` on requeue/visibility expiry. Transport is responsible for retry counting and DLQ decisions when requested by `HandleProcessingErrorAsync`.
- **DLQ behavior**: DLQ is a transport-owned queue/key. DLQ entries include the original envelope and failure metadata (exception message, lastAttemptAt). Default DLQ key prefix: `{TransportName}:{QueueName}:dlq`.
- **DI registration rules**: Transports register open-generic `IMessageQueueTransport<>`. The `ITransportResolver<T>` collects transports by their `TransportType`. If more than one implementation registers the same `TransportType` for the same `T`, startup SHOULD throw.
- **Resolver lifetime**: Register `TransportResolver<T>` as an open-generic singleton. Transports may be singletons if thread-safe; otherwise manage internal concurrency.
- **Serialization**: Use `System.Text.Json` with tolerant settings (camelCase, ignore unknown). Envelope should include `TypeName` and `SchemaVersion` metadata; DTOs must provide a parameterless ctor and a full-field ctor.
- **Identity & correlation**: `Envelope.Id` is a GUID string. Include optional `CorrelationId` and `CausationId` fields. Always log `Envelope.Id` and `CorrelationId` when present.
- **Ordering**: No global ordering guarantee. Document per-transport ordering semantics explicitly (e.g., Redis list FIFO with single consumer; concurrent consumers can break ordering).
- **Concurrency & shutdown**: Workers use `SemaphoreSlim(MaxDegreeOfParallelism)` from `WorkerQueueSettings:{Name}:MaxDegreeOfParallelism`. On shutdown stop fetching new items and wait for in-flight tasks up to host graceful timeout before cancelling.
- **Errors & exceptions**: Facade throws `TransportNotRegisteredException` when an explicit transport is requested but unavailable. Define `TransportInitializationException` and `QueueProcessingException` for clarity.
- **Backoff defaults**: Default = exponential backoff with full jitter. Defaults: initial 100ms, factor 2, max 30s. Configurable via `Queue:Retry:*`.
- **Metrics & logs**: Canonical metrics: `queue.enqueued`, `queue.dequeued`, `queue.processing.duration`, `queue.retries`, `queue.deadletter.count`. Log fields: `worker`, `transport`, `envelopeId`, `messageType`, `attempt`, `exception`.
- **Testing strategy**: Unit test resolver and facade with fakes; integration tests use docker-compose (Redis) to validate end-to-end flow including DLQ.
- **Helper naming**: Use `Add{Transport}MessageTransport` convention for DI helpers (e.g., `AddInMemoryMessageTransport`, `AddRedisMessageTransport`).
- **Payload size**: Default max payload 256KB; larger payloads should be stored externally (blob store) with a pointer in the envelope. Configurable via `Queue:MaxPayloadBytes`.
- **Security**: Transports must support secure connection options (TLS, auth) and secrets should come from secure config providers (KeyVault, etc.).
- **Versioning**: Envelope must include `SchemaVersion`; prefer additive schema changes and tolerant deserialization.

These defaults are intended to minimize ambiguity during implementation. If a team decision deviates from a default, record that decision in the doc and the reason.

## Observability and Metrics

- Log levels:
  - Info: enqueue/dequeue successes, worker start/stop
  - Warning: retries, transient errors
  - Error: processing exceptions that lead to DLQ
- Emit metrics: `queue.enqueued`, `queue.dequeued`, `queue.processing.duration`, `queue.retries`, `queue.deadletter.count`.
- Correlate logs with request or message ids when available.

---

## Testing & Acceptance

- Unit tests: queue enqueue/dequeue semantics, envelope metadata mutation, HandleProcessingError behavior (increment delivery count, requeue, DLQ move).
- Integration tests: run Redis in docker, verify worker processes messages, retry/backoff behavior, and DLQ movement.
- Acceptance tests: end-to-end flow enqueue->process, simulate handler failures up to retry limit and confirm DLQ receives message and worker does not loop forever.

---

---