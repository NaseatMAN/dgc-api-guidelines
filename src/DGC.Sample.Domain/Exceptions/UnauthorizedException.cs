using System.Net;
using DGC.Sample.Domain.Constants;

namespace DGC.Sample.Domain.Exceptions;

public sealed class UnauthorizedException : ApiException
{
    public UnauthorizedException(string message)
        : base(ErrorCodes.Unauthorized, (int)HttpStatusCode.Unauthorized, message)
    {
    }
}
