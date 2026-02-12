using System.Net;
using DGC.Sample.Domain.Exceptions.Errors;

namespace DGC.Sample.Domain.Exceptions;

public sealed class ConflictException : ApiException
{
    public ConflictException(AzureError error) 
        : base(HttpStatusCode.Conflict, error) 
        {
            if (ConflictErrorCode.IsValidCode(error.Code) is false)
                throw new ArgumentException($"Invalid error code for ConflictException: {error.Code}");
        }

    public ConflictException(string code, string message) 
        : base(HttpStatusCode.Conflict, code, message) 
        {
            if (ConflictErrorCode.IsValidCode(code) is false)
                throw new ArgumentException($"Invalid error code for ConflictException: {code}");
        }
}
