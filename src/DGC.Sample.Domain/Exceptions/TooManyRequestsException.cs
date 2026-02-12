using System.Net;
using DGC.Sample.Domain.Exceptions.Errors;

namespace DGC.Sample.Domain.Exceptions;

public sealed class TooManyRequestsException : ApiException
{
    public TooManyRequestsException(AzureError error, int retryAfter) 
        : base(HttpStatusCode.TooManyRequests, error, retryAfter) 
        {
            if (TooManyRequestsErrorCode.IsValidCode(error.Code) is false)
                throw new ArgumentException($"Invalid error code for TooManyRequestsException: {error.Code}");
        }

    public TooManyRequestsException(string code, string message, int retryAfter) 
        : base(HttpStatusCode.TooManyRequests, code, message, retryAfter) 
        {
            if (TooManyRequestsErrorCode.IsValidCode(code) is false)
                throw new ArgumentException($"Invalid error code for TooManyRequestsException: {code}");
        }
}
