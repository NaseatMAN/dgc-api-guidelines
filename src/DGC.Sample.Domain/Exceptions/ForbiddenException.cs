using System.Net;
using DGC.Sample.Domain.Exceptions.Errors;

namespace DGC.Sample.Domain.Exceptions;

public sealed class ForbiddenException : ApiException
{
    public ForbiddenException(AzureError error) 
        : base(HttpStatusCode.Forbidden, error) 
        {
            if (ForbiddenErrorCode.IsValidCode(error.Code) is false)
                throw new ArgumentException($"Invalid error code for ForbiddenException: {error.Code}");
        }

    public ForbiddenException(string code, string message) 
        : base(HttpStatusCode.Forbidden, code, message) 
        {
            if (ForbiddenErrorCode.IsValidCode(code) is false)
                throw new ArgumentException($"Invalid error code for ForbiddenException: {code}");
        }
}
