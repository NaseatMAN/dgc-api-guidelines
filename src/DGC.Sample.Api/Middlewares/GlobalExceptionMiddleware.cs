using System.Net;
using System.Text.Json;
using DGC.Sample.Domain.Exceptions;
using DGC.Sample.Domain.Exceptions.Errors;

namespace DGC.Sample.Api.Middlewares;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger,
    IHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger = logger;
    private readonly IHostEnvironment _environment = environment;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            _logger.LogWarning(ex, "API exception occurred with status code {StatusCode}", ex.StatusCode);
            context.Response.StatusCode = ex.StatusCode;
            context.Response.Headers["x-ms-error-code"] = ex.ResponseBody.Error.Code;
            await context.Response.WriteAsJsonAsync(ex.ResponseBody);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.Headers["x-ms-error-code"] = InternalErrorCode.UnexpectedError;
            var errorDetails = new List<AzureErrorDetail>
            {
                new("ExceptionType", ex.GetType().FullName ?? ex.GetType().Name),
                new("Message", ex.Message),
                new("Source", ex.Source ?? "N/A"),
                new("StackTrace", ex.StackTrace ?? "N/A"),
                new("HResult", ex.HResult.ToString()),
                new("TargetSite", ex.TargetSite?.ToString() ?? "N/A"),
                new("HelpLink", ex.HelpLink ?? "N/A")
            };

            // Add exception data if any
            if (ex.Data.Count > 0)
            {
                var dataEntries = new List<string>();
                foreach (var key in ex.Data.Keys)
                {
                    dataEntries.Add($"{key}: {ex.Data[key]}");
                }
                errorDetails.Add(new("Data", string.Join("; ", dataEntries)));
            }

            // Add inner exceptions recursively
            var innerEx = ex.InnerException;
            var innerExceptionLevel = 1;
            while (innerEx != null)
            {
                errorDetails.Add(new($"InnerException[{innerExceptionLevel}].Type", innerEx.GetType().FullName ?? innerEx.GetType().Name));
                errorDetails.Add(new($"InnerException[{innerExceptionLevel}].Message", innerEx.Message));
                errorDetails.Add(new($"InnerException[{innerExceptionLevel}].Source", innerEx.Source ?? "N/A"));
                errorDetails.Add(new($"InnerException[{innerExceptionLevel}].StackTrace", innerEx.StackTrace ?? "N/A"));
                errorDetails.Add(new($"InnerException[{innerExceptionLevel}].HResult", innerEx.HResult.ToString()));
                errorDetails.Add(new($"InnerException[{innerExceptionLevel}].TargetSite", innerEx.TargetSite?.ToString() ?? "N/A"));
                errorDetails.Add(new($"InnerException[{innerExceptionLevel}].HelpLink", innerEx.HelpLink ?? "N/A"));
                innerEx = innerEx.InnerException;
                innerExceptionLevel++;
            }

            var payload = _environment.IsDevelopment() ? 
                new AzureError(InternalErrorCode.UnexpectedError, ex.ToString(), Details: errorDetails) :
                new AzureError(InternalErrorCode.UnexpectedError, "An unexpected error occurred.");

            var payloadWrapper = new AzureErrorResponse(payload);
            await context.Response.WriteAsJsonAsync(payloadWrapper, JsonOptions);
        }
    }
}
