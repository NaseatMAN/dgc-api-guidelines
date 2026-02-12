# Standard Error Handling (RFC 7807)

Use Problem Details for all error responses to provide a machine-readable format for errors. This ensures consistency across all services and aligns with industry standards.

## 1. Requirements

- **Headers:** `Content-Type: application/problem+json`
- **Correlation ID:** Always include a `correlationId` in the response extensions to facilitate log aggregation and debugging.
- **Error Types:** Use the `type` field to link to documentation for specific error types (e.g., `https://api.contoso.gov/problems/validation-error`).

## 2. .NET Implementation

Use the built-in `AddProblemDetails` and `IExceptionHandler` (available in .NET 8+) for centralized mapping and consistent error responses.

### 2.1 Service Registration

Register the problem details services and your custom exception handlers in `Program.cs`.

```csharp
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
```

### 2.2 Middleware Configuration

Ensure the exception handler middleware is used early in the pipeline.

```csharp
app.UseExceptionHandler(); // Automatically produces ProblemDetails
```

### 2.3 Custom Exception Handler Example

Implement `IExceptionHandler` to handle specific domain exceptions or to provide a generic fallback for unhandled errors.

```csharp
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Type = "https://api.contoso.gov/problems/internal-server-error",
            Extensions = { ["correlationId"] = context.TraceIdentifier }
        };

        context.Response.StatusCode = problemDetails.Status.Value;
        await context.Response.WriteAsJsonAsync(problemDetails, ct);

        return true;
    }
}
```

## 3. Validation Errors

When validation fails (e.g., via `FluentValidation` or `DataAnnotations`), return a `400 Bad Request` with an `errors` collection in the extension.

```json
{
  "type": "https://api.contoso.gov/problems/validation-error",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/v1/customers",
  "correlationId": "1d2c4f8a3e7b4b1b8a4f42d8d3e7f99b",
  "errors": [
    {
      "field": "email",
      "message": "Email must be a valid address."
    }
  ]
}
```
