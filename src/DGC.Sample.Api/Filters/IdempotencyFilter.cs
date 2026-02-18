using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DGC.Sample.Infrastructure.Persistence;
using System.Text.Json;

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

        var executedContext = await next();

        if (executedContext.Result is ObjectResult objectResult && objectResult.StatusCode is >= 200 and < 300)
        {
            var responseBody = JsonSerializer.Serialize(objectResult.Value);
            await _idempotencyService.SaveRequestAsync(
                key,
                context.HttpContext.Request.Path,
                objectResult.StatusCode.Value,
                responseBody,
                context.HttpContext.RequestAborted);
            
            context.HttpContext.Response.Headers[IdempotencyKeyHeader] = key;
        }
    }
}
