using System.Collections;
using System.Net;
using DGC.Sample.Domain.Exceptions.Errors;

namespace DGC.Sample.Domain.Exceptions;

public abstract class ApiException : Exception
{
    public ApiException() { throw new NotImplementedException("Use the constructor with parameters."); }

    protected ApiException(HttpStatusCode statusCode, string code, string message, int? retryAfter = null) : base(message)
    {
        StatusCode = (int)statusCode;
        ResponseBody = new AzureErrorResponse(new AzureError(code, message));
        RetryAfter = retryAfter;
    }

    protected ApiException(HttpStatusCode statusCode, AzureError error, int? retryAfter = null) : base(error.Message)
    {
        StatusCode = (int)statusCode;
        ResponseBody = new AzureErrorResponse(error);
        RetryAfter = retryAfter;
    }

    public int StatusCode { get; }
    public AzureErrorResponse ResponseBody { get; }
    public int? RetryAfter { get; }
    
}
