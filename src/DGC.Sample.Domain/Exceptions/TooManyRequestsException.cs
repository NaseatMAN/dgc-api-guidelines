using System.Net;
using DGC.Sample.Domain.Exceptions.Errors;

namespace DGC.Sample.Domain.Exceptions;

public sealed class TooManyRequestsException : ApiException
{
    /// <summary>
    /// Initializes a new instance of <see cref="TooManyRequestsException"/> with an Azure error object.
    /// </summary>
    public TooManyRequestsException(AzureError error, int retryAfter) 
        : base(HttpStatusCode.TooManyRequests, error, retryAfter) 
        {
            if (TooManyRequestsErrorCode.IsValidCode(error.Code) is false)
                throw new ArgumentException($"Invalid error code for TooManyRequestsException: {error.Code}");
        }

    /// <summary>
    /// Initializes a new instance of <see cref="TooManyRequestsException"/> with detailed error information.
    /// </summary>
    public TooManyRequestsException(
        string code,
        string message,
        string? target = null,
        IReadOnlyList<AzureErrorDetail>? azureErrorDetails = null,
        AzureInnerError? azureInnerError = null,
        int? retryAfter = null) 
        : base(HttpStatusCode.TooManyRequests, code, message, target, azureErrorDetails, azureInnerError, retryAfter) 
        {
            if (TooManyRequestsErrorCode.IsValidCode(code) is false)
                throw new ArgumentException($"Invalid error code for TooManyRequestsException: {code}");
        }

    /// <summary>
    /// Initializes a new instance of <see cref="TooManyRequestsException"/> with error details as a dictionary.
    /// </summary>
    public TooManyRequestsException(
        string code,
        string message,
        string? target = null,
        IDictionary<string, string>? errorDetails = null,
        AzureInnerError? azureInnerError = null,
        int? retryAfter = null)
        : base(HttpStatusCode.TooManyRequests, code, message, target, errorDetails, azureInnerError, retryAfter)
    {
        if (TooManyRequestsErrorCode.IsValidCode(code) is false)
            throw new ArgumentException($"Invalid error code for TooManyRequestsException: {code}");
    }

    /// <summary>
    /// Initializes a new instance of <see cref="TooManyRequestsException"/> with error details and inner error information.
    /// </summary>
    /// <remarks>
    /// This is the preferred constructor for creating TooManyRequestsException instances.
    /// Use this constructor when you need to provide error details and inner error information using simple types.
    /// </remarks>
    public TooManyRequestsException(
        string code,
        string message,
        string? target = null,
        IDictionary<string, string>? errorDetails = null,
        string? innerCode = null,
        string? innerMessage = null,
        int? retryAfter = null)
        : base(HttpStatusCode.TooManyRequests, code, message, target, errorDetails, innerCode, innerMessage, retryAfter)
    {
        if (TooManyRequestsErrorCode.IsValidCode(code) is false)
            throw new ArgumentException($"Invalid error code for TooManyRequestsException: {code}");
    }
}
