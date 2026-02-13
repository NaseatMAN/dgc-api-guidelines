using System.Collections.Immutable;
using DGC.Sample.Domain.Exceptions.Errors;
using System.Net;

namespace DGC.Sample.Domain.Exceptions;

public abstract class ApiException : Exception
{
    protected ApiException() { throw new NotImplementedException("Use the constructor with parameters."); }

    protected ApiException(HttpStatusCode statusCode, AzureError error, int? retryAfter = null) : base(error.Message)
    {
        StatusCode = (int)statusCode;
        ResponseBody = new AzureErrorResponse(error);
        RetryAfter = retryAfter;
    }

    protected ApiException(
        HttpStatusCode statusCode,
        string code,
        string message,
        string? target = null,
        IReadOnlyList<AzureErrorDetail>? azureErrorDetails = null,
        AzureInnerError? azureInnerError = null,
        int? retryAfter = null) : base(message)
    {
        var error = new AzureError(code, message, target, azureErrorDetails, azureInnerError);
        var wrappedError = new AzureErrorResponse(error);

        StatusCode = (int)statusCode;
        ResponseBody = wrappedError;
        RetryAfter = retryAfter;
    }

    protected ApiException(
        HttpStatusCode statusCode,
        string code,
        string message,
        string? target = null,
        IDictionary<string, string>? errorDetails = null,
        AzureInnerError? azureInnerError = null,
        int? retryAfter = null) : base(message)
    {
        var azureErrorDetails = errorDetails?
            .Select(pair => new AzureErrorDetail(pair.Key, pair.Value))
            .ToImmutableList();

        var error = new AzureError(code, message, target, azureErrorDetails, azureInnerError);
        var wrappedError = new AzureErrorResponse(error);

        StatusCode = (int)statusCode;
        ResponseBody = wrappedError;
        RetryAfter = retryAfter;
    }

    protected ApiException(
        HttpStatusCode statusCode,
        string code,
        string message,
        string? target = null,
        IDictionary<string, string>? errorDetails = null,
        string? innerCode = null,
        string? innerMessage = null,
        int? retryAfter = null) : base(message)
    {
        var azureErrorDetails = errorDetails?
            .Select(pair => new AzureErrorDetail(pair.Key, pair.Value))
            .ToImmutableList();

        var azureInnerError = innerCode != null
            ? new AzureInnerError(innerCode, innerMessage) 
            : null;

        var error = new AzureError(code, message, target, azureErrorDetails, azureInnerError);
        var wrappedError = new AzureErrorResponse(error);

        StatusCode = (int)statusCode;
        ResponseBody = wrappedError;
        RetryAfter = retryAfter;
    }

    public int StatusCode { get; }
    public AzureErrorResponse ResponseBody { get; }
    public int? RetryAfter { get; }
}
