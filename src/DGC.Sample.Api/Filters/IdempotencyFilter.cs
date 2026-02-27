using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;
using DGC.Sample.Application.Interfaces.Persistence;
using DGC.Sample.Domain.Exceptions;

namespace DGC.Sample.Api.Filters;

public sealed class IdempotencyFilter(IIdempotencyService idempotencyService) : IAsyncActionFilter
{
    private const string IdempotencyKeyHeader = "Idempotency-Key";
    private readonly IIdempotencyService _idempotencyService = idempotencyService;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var idempotencyKey) || string.IsNullOrEmpty(idempotencyKey))
        {
            await next();
            return;
        }

        var key = idempotencyKey.ToString();
        var existingRequest = await _idempotencyService.GetRequestAsync(key, context.HttpContext.RequestAborted);

        if (existingRequest != null)
        {
            if (existingRequest.IsProcessing)
            {
                // A request with this key is currently being processed
                throw new ConflictException(
                    code: ConflictErrorCode.IdempotencyKeyProcessing,
                    message: "A request with the same idempotency key is currently being processed.",
                    azureErrorDetails: null);
            }

            context.HttpContext.Response.Headers[IdempotencyKeyHeader] = key;
            context.HttpContext.Response.Headers["Repeatability-Result"] = "accepted"; // Consistent with Azure LRO/Repeatability patterns

            var result = new ContentResult
            {
                Content = existingRequest.ResponseBody,
                ContentType = "application/json",
                StatusCode = existingRequest.StatusCode
            };
            context.Result = result;
            return;
        }

        // Try to atomically reserve the key
        if (!await _idempotencyService.TryStartRequestAsync(key, context.HttpContext.RequestAborted))
        {
            // Re-check in case it finished between Get and TryStart
            var finalCheck = await _idempotencyService.GetRequestAsync(key, context.HttpContext.RequestAborted);
            if (finalCheck != null && !finalCheck.IsProcessing)
            {
                context.HttpContext.Response.Headers[IdempotencyKeyHeader] = key;
                context.HttpContext.Response.Headers["Repeatability-Result"] = "accepted";
                context.Result = new ContentResult
                {
                    Content = finalCheck.ResponseBody,
                    ContentType = "application/json",
                    StatusCode = finalCheck.StatusCode
                };
                return;
            }

            throw new ConflictException(
                code: ConflictErrorCode.IdempotencyKeyProcessing,
                message: "A request with the same idempotency key is currently being processed.",
                azureErrorDetails: null);
        }

        var executedContext = await next();

        if (executedContext.Result is ObjectResult objectResult && objectResult.StatusCode is >= 200 and < 300)
        {
            var responseBody = JsonSerializer.Serialize(objectResult.Value);
            await _idempotencyService.SaveRequestAsync(
                key,
                objectResult.StatusCode.Value,
                responseBody,
                context.HttpContext.RequestAborted);

            context.HttpContext.Response.Headers[IdempotencyKeyHeader] = key;
        }
    }
}
