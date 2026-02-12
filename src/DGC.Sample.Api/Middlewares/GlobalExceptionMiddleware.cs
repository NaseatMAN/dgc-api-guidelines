using System.Net;
using System.Text.Json;
using DGC.Sample.Api.Errors;
using DGC.Sample.Domain.Constants;
using DGC.Sample.Domain.Exceptions;

namespace DGC.Sample.Api.Middlewares;

public sealed class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            var (statusCode, error) = MapException(ex, context);

            if (statusCode >= (int)HttpStatusCode.InternalServerError)
            {
                _logger.LogError(ex, "Unhandled exception");
            }
            else
            {
                _logger.LogWarning(ex, "Request failed with {StatusCode}", statusCode);
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            context.Response.Headers["x-ms-error-code"] = error.Code;

            var payload = new AzureErrorResponse(error);
            await JsonSerializer.SerializeAsync(context.Response.Body, payload, JsonOptions);
        }
    }

    private (int StatusCode, AzureError Error) MapException(Exception ex, HttpContext context)
    {
        var traceId = context.TraceIdentifier;

        return ex switch
        {
            ApiException apiException => (
                apiException.StatusCode,
                new AzureError(
                    Code: apiException.Code,
                    Message: apiException.Message,
                    InnerError: new AzureInnerError(TraceId: traceId))),
            ArgumentNullException argNull => (
                (int)HttpStatusCode.BadRequest,
                new AzureError(
                    Code: ErrorCodes.BadRequest,
                    Message: argNull.Message,
                    InnerError: new AzureInnerError(TraceId: traceId))),
            ArgumentException arg => (
                (int)HttpStatusCode.BadRequest,
                new AzureError(
                    Code: ErrorCodes.BadRequest,
                    Message: arg.Message,
                    InnerError: new AzureInnerError(TraceId: traceId))),
            UnauthorizedAccessException => (
                (int)HttpStatusCode.Unauthorized,
                new AzureError(
                    Code: ErrorCodes.Unauthorized,
                    Message: "Authentication is required to access this resource.",
                    InnerError: new AzureInnerError(TraceId: traceId))),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                new AzureError(
                    Code: ErrorCodes.InternalServerError,
                    Message: _environment.IsDevelopment()
                        ? ex.Message
                        : "An unexpected error occurred.",
                    InnerError: new AzureInnerError(TraceId: traceId)))
        };
    }
}
