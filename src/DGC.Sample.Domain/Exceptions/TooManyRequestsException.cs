using System.Net;
using DGC.Sample.Domain.Constants;

namespace DGC.Sample.Domain.Exceptions;

public sealed class TooManyRequestsException : ApiException
{
    public TooManyRequestsException(string message)
        : base(ErrorCodes.TooManyRequests, 429, message)
    {
    }
}
