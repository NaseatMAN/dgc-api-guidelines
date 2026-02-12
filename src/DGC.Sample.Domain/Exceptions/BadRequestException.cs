using System.Net;
using DGC.Sample.Domain.Constants;

namespace DGC.Sample.Domain.Exceptions;

public sealed class BadRequestException : ApiException
{
    public BadRequestException(string message)
        : base(ErrorCodes.BadRequest, (int)HttpStatusCode.BadRequest, message)
    {
    }
}
