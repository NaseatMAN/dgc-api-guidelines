using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Text.Json;
using System.Text;
using DGC.Sample.Application.Interfaces.Persistence;
using DGC.Sample.Application.Dtos;
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
        var requestHash = ComputeRequestHash(context);
        var execution = await _idempotencyService.TryStartRequestAsync(key, requestHash, context.HttpContext.RequestAborted);

        switch (execution.State)
        {
            case IdempotencyExecutionState.Completed:
                WriteReplayHeaders(context.HttpContext.Response, key);
                context.Result = new ContentResult
                {
                    Content = execution.CachedResponse!.ResponseBody,
                    ContentType = execution.CachedResponse.ContentType,
                    StatusCode = execution.CachedResponse.StatusCode
                };
                return;
            case IdempotencyExecutionState.Processing:
                throw new ConflictException(
                    code: ConflictErrorCode.IdempotencyKeyProcessing,
                    message: "A request with the same idempotency key is currently being processed.",
                    azureErrorDetails: null);
            case IdempotencyExecutionState.RequestMismatch:
                throw new ConflictException(
                    code: ConflictErrorCode.IdempotencyKeyReuseMismatch,
                    message: "The same idempotency key cannot be reused with a different request payload.",
                    azureErrorDetails: null);
        }

        try
        {
            var executedContext = await next();
            var cachedResponse = TryBuildCacheEntry(executedContext.Result);
            if (cachedResponse is not null)
            {
                await _idempotencyService.SaveRequestAsync(
                    key,
                    requestHash,
                    cachedResponse.Value.StatusCode,
                    cachedResponse.Value.ResponseBody,
                    cachedResponse.Value.ContentType,
                    context.HttpContext.RequestAborted);

                context.HttpContext.Response.Headers[IdempotencyKeyHeader] = key;
            }
        }
        catch
        {
            await _idempotencyService.ReleaseRequestAsync(key, requestHash, context.HttpContext.RequestAborted);
            throw;
        }
    }

    private static void WriteReplayHeaders(HttpResponse response, string key)
    {
        response.Headers[IdempotencyKeyHeader] = key;
        response.Headers["Repeatability-Result"] = "accepted";
    }

    private static string ComputeRequestHash(ActionExecutingContext context)
    {
        var payload = new SortedDictionary<string, object?>(StringComparer.Ordinal);
        foreach (var argument in context.ActionArguments)
        {
            if (!ShouldIncludeInHash(argument.Value))
            {
                continue;
            }

            payload[argument.Key] = argument.Value;
        }

        var canonicalRequest = JsonSerializer.Serialize(new
        {
            method = context.HttpContext.Request.Method,
            path = context.HttpContext.Request.Path.Value,
            query = context.HttpContext.Request.QueryString.Value,
            arguments = payload
        });

        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest)));
    }

    private static bool ShouldIncludeInHash(object? argumentValue)
    {
        if (argumentValue is null)
        {
            return true;
        }

        return argumentValue switch
        {
            CancellationToken => false,
            HttpContext => false,
            HttpRequest => false,
            HttpResponse => false,
            ClaimsPrincipal => false,
            Stream => false,
            _ => true
        };
    }

    private static (int StatusCode, string ResponseBody, string ContentType)? TryBuildCacheEntry(IActionResult? result)
    {
        switch (result)
        {
            case ObjectResult objectResult when IsSuccessful(objectResult.StatusCode):
                return (
                    objectResult.StatusCode!.Value,
                    JsonSerializer.Serialize(objectResult.Value),
                    "application/json");
            case JsonResult jsonResult when IsSuccessful(jsonResult.StatusCode):
                return (
                    jsonResult.StatusCode!.Value,
                    JsonSerializer.Serialize(jsonResult.Value),
                    jsonResult.ContentType ?? "application/json");
            case ContentResult contentResult when IsSuccessful(contentResult.StatusCode):
                return (
                    contentResult.StatusCode!.Value,
                    contentResult.Content ?? string.Empty,
                    contentResult.ContentType ?? "text/plain");
            case IStatusCodeActionResult statusCodeResult when IsSuccessful(statusCodeResult.StatusCode):
                return (
                    statusCodeResult.StatusCode!.Value,
                    string.Empty,
                    "text/plain");
            default:
                return null;
        }
    }

    private static bool IsSuccessful(int? statusCode) => statusCode is >= 200 and < 300;
}
