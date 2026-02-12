using System.Net;
using DGC.Sample.Domain.Constants;

namespace DGC.Sample.Domain.Exceptions;

public sealed class NotFoundException : ApiException
{
    public NotFoundException(string message)
        : base(ErrorCodes.NotFound, (int)HttpStatusCode.NotFound, message)
    {
    }
}
