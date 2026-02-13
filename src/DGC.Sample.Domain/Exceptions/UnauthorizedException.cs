using System.Net;
using DGC.Sample.Domain.Exceptions.Errors;

namespace DGC.Sample.Domain.Exceptions;

public sealed class UnauthorizedException : ApiException
{
    /// <summary>
    /// Initializes a new instance of <see cref="UnauthorizedException"/> with an Azure error object.
    /// </summary>
    public UnauthorizedException(AzureError error) 
        : base(HttpStatusCode.Unauthorized, error) 
        {
            if (UnauthorizedErrorCode.IsValidCode(error.Code) is false)
                throw new ArgumentException($"Invalid error code for UnauthorizedException: {error.Code}");
        }

    /// <summary>
    /// Initializes a new instance of <see cref="UnauthorizedException"/> with detailed error information.
    /// </summary>
    public UnauthorizedException(
        string code,
        string message,
        string? target = null,
        IReadOnlyList<AzureErrorDetail>? azureErrorDetails = null,
        AzureInnerError? azureInnerError = null,
        int? retryAfter = null) 
        : base(HttpStatusCode.Unauthorized, code, message, target, azureErrorDetails, azureInnerError, retryAfter) 
        {
            if (UnauthorizedErrorCode.IsValidCode(code) is false)
                throw new ArgumentException($"Invalid error code for UnauthorizedException: {code}");
        }

    /// <summary>
    /// Initializes a new instance of <see cref="UnauthorizedException"/> with error details as a dictionary.
    /// </summary>
    public UnauthorizedException(
        string code,
        string message,
        string? target = null,
        IDictionary<string, string>? errorDetails = null,
        AzureInnerError? azureInnerError = null,
        int? retryAfter = null)
        : base(HttpStatusCode.Unauthorized, code, message, target, errorDetails, azureInnerError, retryAfter)
    {
        if (UnauthorizedErrorCode.IsValidCode(code) is false)
            throw new ArgumentException($"Invalid error code for UnauthorizedException: {code}");
    }

    /// <summary>
    /// Initializes a new instance of <see cref="UnauthorizedException"/> with error details and inner error information.
    /// </summary>
    /// <remarks>
    /// This is the preferred constructor for creating UnauthorizedException instances.
    /// Use this constructor when you need to provide error details and inner error information using simple types.
    /// </remarks>
    public UnauthorizedException(
        string code,
        string message,
        string? target = null,
        IDictionary<string, string>? errorDetails = null,
        string? innerCode = null,
        string? innerMessage = null,
        int? retryAfter = null)
        : base(HttpStatusCode.Unauthorized, code, message, target, errorDetails, innerCode, innerMessage, retryAfter)
    {
        if (UnauthorizedErrorCode.IsValidCode(code) is false)
            throw new ArgumentException($"Invalid error code for UnauthorizedException: {code}");
    }
}
