using DGC.Sample.Domain.Exceptions.Errors;
using DGC.Sample.Domain.Constants.ApiErrorConstants;
using System.Text.Json;
using DGC.Sample.Domain.Exceptions;

namespace DGC.Sample.Api.Extensions;

public sealed class AzureProblemDetailsWriter : IProblemDetailsWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public bool CanWrite(ProblemDetailsContext context)
    {
        // We want to handle all ProblemDetails and convert them to Azure Error Response format.
        return true;
    }

    public async ValueTask WriteAsync(ProblemDetailsContext context)
    {
        var problem = context.ProblemDetails;
        var httpContext = context.HttpContext;

        // Map ProblemDetails to AzureError
        var errorCode = problem.Extensions.TryGetValue("code", out var code) && code is string s ? s : null;
        var isVersioningError = false;

        if (string.IsNullOrEmpty(errorCode))
        {
            // Try to extract code from type URL if it's a versioning error
            // New format: https://docs.api-versioning.org/problems#unspecified
            if (problem.Type != null && (problem.Type.Contains("api-versioning") || problem.Type.Contains("aspnet-api-versioning")))
            {
                var typeFragment = problem.Type.Split('#').LastOrDefault();
                errorCode = typeFragment switch
                {
                    "unspecified" => VersioningErrorCode.MissingApiVersionParameter,
                    "apiVersionUnspecified" => VersioningErrorCode.MissingApiVersionParameter,
                    "unsupported" => VersioningErrorCode.UnsupportedApiVersionValue,
                    "unsupportedApiVersion" => VersioningErrorCode.UnsupportedApiVersionValue,
                    _ => typeFragment
                };
                isVersioningError = true;
            }
            else
            {
                errorCode = problem.Status?.ToString() ?? "InternalServerError";
            }
        }
        else if (errorCode == VersioningErrorCode.MissingApiVersionParameter || errorCode == VersioningErrorCode.UnsupportedApiVersionValue)
        {
            isVersioningError = true;
        }

        var message = problem.Detail ?? problem.Title ?? "An error occurred.";

        // Override messages for versioning errors to match Azure Guidelines exactly
        if (isVersioningError)
        {
            if (errorCode == VersioningErrorCode.MissingApiVersionParameter)
            {
                message = "The api-version query parameter (?api-version=) is required for all requests";
            }
            else if (errorCode == VersioningErrorCode.UnsupportedApiVersionValue)
            {
                // For unsupported version, try to get the requested version if available
                var requestedVersion = problem.Extensions.TryGetValue("apiVersion", out var v) ? v?.ToString() : null;

                if (string.IsNullOrEmpty(requestedVersion))
                {
                    // Fallback to query string
                    requestedVersion = httpContext.Request.Query["api-version"].ToString();
                }

                if (string.IsNullOrEmpty(requestedVersion))
                {
                    requestedVersion = "unknown";
                }

                message = $"Unsupported api-version '{requestedVersion}'.";
            }

            // Throw BadRequestException to be caught by GlobalExceptionMiddleware
            throw new BadRequestException(
                code: errorCode!,
                message: message,
                target: problem.Instance,
                azureErrorDetails: null,
                azureInnerError: new AzureInnerError(TraceId: httpContext.TraceIdentifier));
        }

        var azureError = new AzureError(
        Code: errorCode!,
        Message: message,
        Target: problem.Instance,
        InnerError: new AzureInnerError(TraceId: httpContext.TraceIdentifier));

        // Ensure x-ms-error-code header is set
        httpContext.Response.Headers["x-ms-error-code"] = azureError.Code;

        // We don't set status code here because it's already set by the caller of IProblemDetailsWriter usually.
        // But let's be safe.
        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";

        var response = new AzureErrorResponse(azureError);
        await httpContext.Response.WriteAsJsonAsync(response, JsonOptions);
    }
}
