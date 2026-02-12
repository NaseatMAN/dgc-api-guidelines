using System.Net;
using DGC.Sample.Domain.Exceptions.Errors;

namespace DGC.Sample.Domain.Exceptions;

public sealed class UnprocessableEntityException : ApiException
{
    public UnprocessableEntityException(AzureError error, int? retryAfter = null) 
        : base(HttpStatusCode.UnprocessableEntity, error, retryAfter) 
        {
            if (UnprocessableEntityErrorCode.IsValidCode(error.Code) is false)
                throw new ArgumentException($"Invalid error code for UnprocessableEntityException: {error.Code}");
        }

    public UnprocessableEntityException(string code, string message, int? retryAfter = null) 
        : base(HttpStatusCode.UnprocessableEntity, code, message, retryAfter) 
        {
            if (UnprocessableEntityErrorCode.IsValidCode(code) is false)
                throw new ArgumentException($"Invalid error code for UnprocessableEntityException: {code}");
        }
}
