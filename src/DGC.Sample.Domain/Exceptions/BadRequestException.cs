using System.Net;
using DGC.Sample.Domain.Exceptions.Errors;

namespace DGC.Sample.Domain.Exceptions;

public sealed class BadRequestException : ApiException
{
    public BadRequestException(AzureError error)
        : base(HttpStatusCode.BadRequest, error)
        {
            if (BadRequestErrorCode.IsValidCode(error.Code) is false)
                throw new ArgumentException($"Invalid error code for BadRequestException: {error.Code}");
        }

    public BadRequestException(string code, string message)
        : base(HttpStatusCode.BadRequest, code, message) 
        { 
            if (BadRequestErrorCode.IsValidCode(code) is false)
                throw new ArgumentException($"Invalid error code for BadRequestException: {code}");
        }
}
