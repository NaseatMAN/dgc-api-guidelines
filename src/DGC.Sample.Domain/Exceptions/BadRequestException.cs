using System.Net;
using DGC.Sample.Domain.Constants;

namespace DGC.Sample.Domain.Exceptions;

public sealed class BadRequestException : ApiException
{
    /// <summary>
    /// Initializes a new instance of <see cref="BadRequestException"/> with an Azure error object.
    /// </summary>
    public BadRequestException(AzureError error)
        : base(HttpStatusCode.BadRequest, error)
        {
            if (BadRequestErrorCode.IsValidCode(error.Code) is false)
                throw new ArgumentException($"Invalid error code for BadRequestException: {error.Code}");
        }

    /// <summary>
    /// Initializes a new instance of <see cref="BadRequestException"/> with detailed error information.
    /// </summary>
    public BadRequestException(
        string code,
        string message,
        string? target = null,
        IReadOnlyList<AzureErrorDetail>? azureErrorDetails = null,
        AzureInnerError? azureInnerError = null,
        int? retryAfter = null)
        : base(HttpStatusCode.BadRequest, code, message, target, azureErrorDetails, azureInnerError, retryAfter)
    {
        if (BadRequestErrorCode.IsValidCode(code) is false)
            throw new ArgumentException($"Invalid error code for BadRequestException: {code}");
    }

    /// <summary>
    /// Initializes a new instance of <see cref="BadRequestException"/> with error details as a dictionary.
    /// </summary>
    public BadRequestException(
        string code,
        string message,
        string? target = null,
        IDictionary<string, string>? errorDetails = null,
        AzureInnerError? azureInnerError = null,
        int? retryAfter = null)
        : base(HttpStatusCode.BadRequest, code, message, target, errorDetails, azureInnerError, retryAfter)
    {
        if (BadRequestErrorCode.IsValidCode(code) is false)
            throw new ArgumentException($"Invalid error code for BadRequestException: {code}");
    }

    /// <summary>
    /// Initializes a new instance of <see cref="BadRequestException"/> with error details and inner error information.
    /// </summary>
    /// <remarks>
    /// This is the preferred constructor for creating BadRequestException instances.
    /// Use this constructor when you need to provide error details and inner error information using simple types.
    /// </remarks>
    public BadRequestException(
        string code,
        string message,
        string? target = null,
        IDictionary<string, string>? errorDetails = null,
        string? innerCode = null,
        string? innerMessage = null,
        int? retryAfter = null)
        : base(HttpStatusCode.BadRequest, code, message, target, errorDetails, innerCode, innerMessage, retryAfter)
    {
        if (BadRequestErrorCode.IsValidCode(code) is false)
            throw new ArgumentException($"Invalid error code for BadRequestException: {code}");
    }
}
