using System.Net;
using DGC.Sample.Domain.Constants;

namespace DGC.Sample.Domain.Exceptions;

public sealed class ConflictException : ApiException
{
    public ConflictException(string message)
        : base(ErrorCodes.Conflict, (int)HttpStatusCode.Conflict, message)
    {
    }
}
