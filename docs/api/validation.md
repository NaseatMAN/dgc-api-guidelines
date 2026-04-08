# Input Validation with Custom Validation Attributes

We use custom `ValidationAttribute` implementations (overriding `IsValid`) for request DTO validation.

## Implementation Overview

1. **Custom Attributes**: Defined in `DGC.Sample.Application/Validators/`.
2. **DTOs**: Request DTOs are decorated with validation attributes.
3. **Automatic Validation**: Executed by ASP.NET Core model validation for `[ApiController]` endpoints.
4. **Error Responses**: Validation failures are mapped to the Azure REST API error envelope via `ApiBehaviorOptions.InvalidModelStateResponseFactory`.

## Creating a Custom Validation Attribute

```csharp
using System.ComponentModel.DataAnnotations;

namespace DGC.Sample.Application.Validators;

public sealed class NonDefaultDateTimeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime dateTime && dateTime != default)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
    }
}
```

## Applying Attributes to DTOs

```csharp
public sealed class OrderCreateRequest
{
    [NotWhiteSpace]
    [StringLengthRange(2, 200)]
    public string CustomerName { get; init; } = string.Empty;

    [NonDefaultDateTime]
    public DateTime OrderDateUtc { get; init; }
}
```

For positional records, apply attributes with the `property:` target.

## Error Response Format

When validation fails, the API returns a `400 Bad Request` in the Azure-style envelope:

```json
{
  "error": {
    "code": "invalid_document",
    "message": "One or more validation errors occurred.",
    "details": [
      {
        "code": "invalid_document.CustomerName",
        "message": "CustomerName is required.",
        "target": "CustomerName"
      }
    ],
    "innererror": {
      "traceId": "0HMA12345678"
    }
  }
}
```

This keeps validation behavior aligned with the Microsoft Azure REST API Guidelines.
