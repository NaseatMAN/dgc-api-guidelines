# Observability: Logging, Tracing, and Metrics

This document describes the observability behavior used in the current repository.

## 1. Core Principles

- **Request IDs:** Return `x-ms-request-id` for each request.
- **Logging:** Use structured `ILogger` message templates so logs remain queryable.
- **Traceability:** Keep request identifiers available in error responses and operational logs.

## 2. Current .NET Configuration

The current API uses the built-in logging abstractions and request ID middleware.

### 2.1 Service Registration

```csharp
builder.Services.AddApiControllersWithAzureValidation();

var app = builder.Build();
app.UseApiMiddlewares();
```

## 3. Request ID Behavior

Request identifiers are used to correlate request handling and error output.

- The API sets `x-ms-request-id` on responses.
- Validation and exception responses preserve request trace information.
- Log messages should include contextual identifiers when useful for troubleshooting.

## 4. Logging Guidance

Use message templates instead of string concatenation so structured values remain searchable.

```csharp
logger.LogInformation("Processing order {OrderId} for customer {CustomerId}", orderId, customerId);
```

**Correct:**

`logger.LogInformation("Processing order {OrderId}", orderId);`

**Incorrect:**

`logger.LogInformation("Processing order " + orderId);`

## 5. Operational Focus

- Track request IDs in diagnostics.
- Review warnings and errors from middleware and service logs.
- Keep error payloads aligned with the Azure-style envelope used by the API.
