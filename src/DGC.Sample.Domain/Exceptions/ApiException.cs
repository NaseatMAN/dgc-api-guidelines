namespace DGC.Sample.Domain.Exceptions;

public abstract class ApiException : Exception
{
    protected ApiException(string code, int statusCode, string message) : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    public string Code { get; }
    public int StatusCode { get; }
}
