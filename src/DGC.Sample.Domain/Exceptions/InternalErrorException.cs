using System.Net;
using DGC.Sample.Domain.Exceptions.Errors;

namespace DGC.Sample.Domain.Exceptions;

public sealed class InternalErrorException : ApiException
{
    public InternalErrorException(AzureError error) 
        : base(HttpStatusCode.InternalServerError, error) 
        {
            if (InternalErrorCode.IsValidCode(error.Code) is false)
                throw new ArgumentException($"Invalid error code for InternalErrorException: {error.Code}");
        }

    public InternalErrorException(string code, string message) 
        : base(HttpStatusCode.InternalServerError, code, message) 
        {
            if (InternalErrorCode.IsValidCode(code) is false)
                throw new ArgumentException($"Invalid error code for InternalErrorException: {code}");
        }
}
