using System.Net;
using DGC.Sample.Domain.Constants;

namespace DGC.Sample.Domain.Exceptions;

public sealed class UnprocessableEntityException : ApiException
{
    public UnprocessableEntityException(string message)
        : base(ErrorCodes.UnprocessableEntity, 422, message)
    {
    }
}
