using System.Net;
using DGC.Sample.Domain.Exceptions.Errors;

namespace DGC.Sample.Domain.Exceptions;

public sealed class UnauthorizedException : ApiException
{
    public UnauthorizedException(AzureError error) 
        : base(HttpStatusCode.Unauthorized, error) 
        {
            if (UnauthorizedErrorCode.IsValidCode(error.Code) is false)
                throw new ArgumentException($"Invalid error code for UnauthorizedException: {error.Code}");
        }

    public UnauthorizedException(string code, string message) 
        : base(HttpStatusCode.Unauthorized, code, message) 
        {
            if (UnauthorizedErrorCode.IsValidCode(code) is false)
                throw new ArgumentException($"Invalid error code for UnauthorizedException: {code}");
        }
}
