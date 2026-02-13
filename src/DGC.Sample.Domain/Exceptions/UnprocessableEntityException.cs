using System.Net;
using DGC.Sample.Domain.Exceptions.Errors;

namespace DGC.Sample.Domain.Exceptions;

public sealed class UnprocessableEntityException : ApiException
{
    /// <summary>
    /// Initializes a new instance of <see cref="UnprocessableEntityException"/> with an Azure error object.
    /// </summary>
    public UnprocessableEntityException(AzureError error, int? retryAfter = null) 
        : base(HttpStatusCode.UnprocessableEntity, error, retryAfter) 
        {
            if (UnprocessableEntityErrorCode.IsValidCode(error.Code) is false)
                throw new ArgumentException($"Invalid error code for UnprocessableEntityException: {error.Code}");
        }

    /// <summary>
    /// Initializes a new instance of <see cref="UnprocessableEntityException"/> with detailed error information.
    /// </summary>
    public UnprocessableEntityException(
        string code,
        string message,
        string? target = null,
        IReadOnlyList<AzureErrorDetail>? azureErrorDetails = null,
        AzureInnerError? azureInnerError = null,
        int? retryAfter = null) 
        : base(HttpStatusCode.UnprocessableEntity, code, message, target, azureErrorDetails, azureInnerError, retryAfter) 
        {
            if (UnprocessableEntityErrorCode.IsValidCode(code) is false)
                throw new ArgumentException($"Invalid error code for UnprocessableEntityException: {code}");
        }

    /// <summary>
    /// Initializes a new instance of <see cref="UnprocessableEntityException"/> with error details as a dictionary.
    /// </summary>
    public UnprocessableEntityException(
        string code,
        string message,
        string? target = null,
        IDictionary<string, string>? errorDetails = null,
        AzureInnerError? azureInnerError = null,
        int? retryAfter = null)
        : base(HttpStatusCode.UnprocessableEntity, code, message, target, errorDetails, azureInnerError, retryAfter)
    {
        if (UnprocessableEntityErrorCode.IsValidCode(code) is false)
            throw new ArgumentException($"Invalid error code for UnprocessableEntityException: {code}");
    }

    /// <summary>
    /// Initializes a new instance of <see cref="UnprocessableEntityException"/> with error details and inner error information.
    /// </summary>
    /// <remarks>
    /// This is the preferred constructor for creating UnprocessableEntityException instances.
    /// Use this constructor when you need to provide error details and inner error information using simple types.
    /// </remarks>
    public UnprocessableEntityException(
        string code,
        string message,
        string? target = null,
        IDictionary<string, string>? errorDetails = null,
        string? innerCode = null,
        string? innerMessage = null,
        int? retryAfter = null)
        : base(HttpStatusCode.UnprocessableEntity, code, message, target, errorDetails, innerCode, innerMessage, retryAfter)
    {
        if (UnprocessableEntityErrorCode.IsValidCode(code) is false)
            throw new ArgumentException($"Invalid error code for UnprocessableEntityException: {code}");
    }
}
