namespace DGC.Sample.Api.Middlewares;

public sealed class RequestIdMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context)
    {
        // "DO create an opaque value that uniquely identifies the request and return this value in the x-ms-request-id response header."
        // Using TraceIdentifier which is already generated per request.
        var requestId = context.TraceIdentifier;
        context.Response.Headers["x-ms-request-id"] = requestId;

        await _next(context);
    }
}
