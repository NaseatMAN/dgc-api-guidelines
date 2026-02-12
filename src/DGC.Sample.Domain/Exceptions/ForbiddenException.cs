using System.Net;
using DGC.Sample.Domain.Constants;

namespace DGC.Sample.Domain.Exceptions;

public sealed class ForbiddenException : ApiException
{
    public ForbiddenException(string message)
        : base(ErrorCodes.Forbidden, (int)HttpStatusCode.Forbidden, message)
    {
    }
}
