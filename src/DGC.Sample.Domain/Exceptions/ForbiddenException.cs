using DGC.Sample.Domain.Exceptions.Errors;
using System.Net;

namespace DGC.Sample.Domain.Exceptions;

public sealed class ForbiddenException : ApiException
{
    /// <summary>
    /// Initializes a new instance of <see cref="ForbiddenException"/> with an Azure error object.
    /// </summary>
    public ForbiddenException(AzureError error) 
        : base(HttpStatusCode.Forbidden, error) 
        {
            if (ForbiddenErrorCode.IsValidCode(error.Code) is false)
                throw new ArgumentException($"Invalid error code for ForbiddenException: {error.Code}");
        }

    /// <summary>
    /// Initializes a new instance of <see cref="ForbiddenException"/> with detailed error information.
    /// </summary>
    public ForbiddenException(
        string code,
        string message,
        string? target = null,
        IReadOnlyList<AzureErrorDetail>? azureErrorDetails = null,
        AzureInnerError? azureInnerError = null,
        int? retryAfter = null) 
        : base(HttpStatusCode.Forbidden, code, message, target, azureErrorDetails, azureInnerError, retryAfter) 
        {
            if (ForbiddenErrorCode.IsValidCode(code) is false)
                throw new ArgumentException($"Invalid error code for ForbiddenException: {code}");
        }

    /// <summary>
    /// Initializes a new instance of <see cref="ForbiddenException"/> with error details as a dictionary.
    /// </summary>
    public ForbiddenException(
        string code,
        string message,
        string? target = null,
        IDictionary<string, string>? errorDetails = null,
        AzureInnerError? azureInnerError = null,
        int? retryAfter = null)
        : base(HttpStatusCode.Forbidden, code, message, target, errorDetails, azureInnerError, retryAfter)
    {
        if (ForbiddenErrorCode.IsValidCode(code) is false)
            throw new ArgumentException($"Invalid error code for ForbiddenException: {code}");
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ForbiddenException"/> with error details and inner error information.
    /// </summary>
    /// <remarks>
    /// This is the preferred constructor for creating ForbiddenException instances.
    /// Use this constructor when you need to provide error details and inner error information using simple types.
    /// </remarks>
    public ForbiddenException(
        string code,
        string message,
        string? target = null,
        IDictionary<string, string>? errorDetails = null,
        string? innerCode = null,
        string? innerMessage = null,
        int? retryAfter = null)
        : base(HttpStatusCode.Forbidden, code, message, target, errorDetails, innerCode, innerMessage, retryAfter)
    {
        if (ForbiddenErrorCode.IsValidCode(code) is false)
            throw new ArgumentException($"Invalid error code for ForbiddenException: {code}");
    }
}
