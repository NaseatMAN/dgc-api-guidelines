using System.Net;
using DGC.Sample.Domain.Exceptions.Errors;

namespace DGC.Sample.Domain.Exceptions;

public sealed class NotFoundException : ApiException
{
    /// <summary>
    /// Initializes a new instance of <see cref="NotFoundException"/> with an Azure error object.
    /// </summary>
    public NotFoundException(AzureError error) 
        : base(HttpStatusCode.NotFound, error) 
        {
            if (NotFoundErrorCode.IsValidCode(error.Code) is false)
                throw new ArgumentException($"Invalid error code for NotFoundException: {error.Code}");
        }

    /// <summary>
    /// Initializes a new instance of <see cref="NotFoundException"/> with detailed error information.
    /// </summary>
    public NotFoundException(
        string code,
        string message,
        string? target = null,
        IReadOnlyList<AzureErrorDetail>? azureErrorDetails = null,
        AzureInnerError? azureInnerError = null,
        int? retryAfter = null) 
        : base(HttpStatusCode.NotFound, code, message, target, azureErrorDetails, azureInnerError, retryAfter) 
        {
            if (NotFoundErrorCode.IsValidCode(code) is false)
                throw new ArgumentException($"Invalid error code for NotFoundException: {code}");
        }

    /// <summary>
    /// Initializes a new instance of <see cref="NotFoundException"/> with error details as a dictionary.
    /// </summary>
    public NotFoundException(
        string code,
        string message,
        string? target = null,
        IDictionary<string, string>? errorDetails = null,
        AzureInnerError? azureInnerError = null,
        int? retryAfter = null)
        : base(HttpStatusCode.NotFound, code, message, target, errorDetails, azureInnerError, retryAfter)
    {
        if (NotFoundErrorCode.IsValidCode(code) is false)
            throw new ArgumentException($"Invalid error code for NotFoundException: {code}");
    }

    /// <summary>
    /// Initializes a new instance of <see cref="NotFoundException"/> with error details and inner error information.
    /// </summary>
    /// <remarks>
    /// This is the preferred constructor for creating NotFoundException instances.
    /// Use this constructor when you need to provide error details and inner error information using simple types.
    /// </remarks>
    public NotFoundException(
        string code,
        string message,
        string? target = null,
        IDictionary<string, string>? errorDetails = null,
        string? innerCode = null,
        string? innerMessage = null,
        int? retryAfter = null)
        : base(HttpStatusCode.NotFound, code, message, target, errorDetails, innerCode, innerMessage, retryAfter)
    {
        if (NotFoundErrorCode.IsValidCode(code) is false)
            throw new ArgumentException($"Invalid error code for NotFoundException: {code}");
    }
}
