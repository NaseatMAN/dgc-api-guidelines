# Input Validation with FluentValidation

We use **FluentValidation** for centralizing input validation logic. This replaces standard `DataAnnotations` to provide more powerful, readable, and maintainable validation rules.

## Implementation Overview

1. **Validators**: Defined in `DGC.Sample.Application/Validators/`.
2. **DTOs**: Cleaned of `DataAnnotation` attributes.
3. **Automatic Validation**: Integrated into the ASP.NET Core pipeline using `FluentValidation.AspNetCore`.
4. **Error Responses**: Validation failures are automatically captured and mapped to the **Azure REST API Error Format** via `ApiBehaviorOptions.InvalidModelStateResponseFactory`.

## Creating a Validator

To create a new validator, inherit from `AbstractValidator<T>` where `T` is your DTO:

```csharp
using FluentValidation;
using DGC.Sample.Application.Dtos;

namespace DGC.Sample.Application.Validators;

public sealed class OrderCreateRequestValidator : AbstractValidator<OrderCreateRequest>
{
    public OrderCreateRequestValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .Length(2, 200);

        RuleFor(x => x.OrderDateUtc)
            .NotEmpty();

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0)
            .LessThanOrEqualTo(1_000_000);
    }
}
```

## Registration

Validators are registered in `ServiceCollectionExtensions.cs` using:

```csharp
services.AddValidatorsFromAssemblyContaining<IOrderService>();
services.AddFluentValidationAutoValidation();
```

## Error Response Format

When a validation fails, the API returns a `400 Bad Request` with the following structure:

```json
{
  "error": {
    "code": "BadArgument",
    "message": "One or more validation errors occurred.",
    "details": [
      {
        "code": "BadArgument.CustomerName",
        "message": "'Customer Name' must not be empty.",
        "target": "CustomerName"
      }
    ],
    "innererror": {
      "traceId": "0HMA12345678"
    }
  }
}
```

This ensures compliance with the **Microsoft Azure REST API Guidelines**.
