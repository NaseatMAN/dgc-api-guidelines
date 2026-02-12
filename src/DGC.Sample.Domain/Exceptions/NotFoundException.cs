using System.Net;
using DGC.Sample.Domain.Exceptions.Errors;

namespace DGC.Sample.Domain.Exceptions;

public sealed class NotFoundException : ApiException
{
    public NotFoundException(AzureError error) 
        : base(HttpStatusCode.NotFound, error) 
        {
            if (NotFoundErrorCode.IsValidCode(error.Code) is false)
                throw new ArgumentException($"Invalid error code for NotFoundException: {error.Code}");
        }

    public NotFoundException(string code, string message) 
        : base(HttpStatusCode.NotFound, code, message) 
        {
            if (NotFoundErrorCode.IsValidCode(code) is false)
                throw new ArgumentException($"Invalid error code for NotFoundException: {code}");
        }
}
