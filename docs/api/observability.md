# Observability: Logging, Tracing, and Metrics

High observability is mandatory for distributed systems. We standardize on **OpenTelemetry (OTel)** for vendor-neutral collection of signals (logs, traces, and metrics).

## 1. Core Principles

- **Tracing:** Capture end-to-end request flow across services using W3C `traceparent` headers.
- **Metrics:** Export RED (Rate, Error, Duration) metrics to monitor the health and performance of the API.
- **Logging:** Use structured logging (message templates) to enable efficient querying and analysis in systems like Azure Monitor or ELK.

## 2. .NET Configuration

Use the OpenTelemetry SDK to instrument ASP.NET Core, HTTP clients, and database drivers (e.g., EF Core).

### 2.1 Service Registration

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter()) // Export to OTLP collector
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddPrometheusExporter()); // Export for Prometheus scraping

app.MapPrometheusScrapingEndpoint();
```

## 3. Correlation IDs

Correlation IDs are essential for tracking a single request as it moves through multiple microservices.

- **Acceptance:** Accept `x-correlation-id` from clients or the API Gateway (APIM).
- **Propagation:** Ensure the ID is propagated to all downstream logs, database commands, and external HTTP calls.
- **W3C Trace Context:** Use `Activity.Current?.Id` for W3C `traceparent` compliance, which is the modern standard for distributed tracing.

## 4. Structured Logging with Serilog

While .NET's built-in `ILogger` supports message templates, we recommend using **Serilog** for its robust enrichment and sink support.

### 4.1 Why Serilog?

- **Enrichment:** Automatically add properties like `MachineName`, `Environment`, and `CorrelationId` to every log.

- **Sinks:** Easily export logs to OpenTelemetry, Azure Application Insights, or Elasticsearch.

### 4.2 Configuration Example

Avoid string concatenation in logs. Use message templates to preserve metadata.

```csharp

// Registration in Program.cs

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig

    .ReadFrom.Configuration(context.Configuration)

    .Enrich.FromLogContext()

    .WriteTo.OpenTelemetry()); // Export to OTel collector



// Usage

logger.LogInformation("Processing order {OrderId} for customer {CustomerId}", orderId, customerId);

```

**Correct:**

`logger.LogInformation("Processing order {OrderId}", orderId);`

**Incorrect:**

`logger.LogInformation("Processing order " + orderId);`

## 5. Metrics & Dashboards

Every service should export standard RED metrics:

- **Rate:** Requests per second.
- **Errors:** Number of failed requests (4xx, 5xx).
- **Duration:** Latency percentiles (P50, P90, P99).
