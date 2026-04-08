using System.Security.Cryptography;
using System.Text;
using DGC.Sample.Api.Extensions;
using DGC.Sample.Domain.Constants.ApiErrorConstants;
using DGC.Sample.Domain.Exceptions.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace DGC.Sample.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class ApiKeyAuthenticationAttribute(string keyGroup) : ActionFilterAttribute
{
    public string KeyGroup { get; } = keyGroup;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var options = context.HttpContext.RequestServices
            .GetRequiredService<IOptions<ApiAuthenticationExtension.ApiKeyAuthenticationOptions>>()
            .Value;

        if (!options.EndpointKeys.TryGetValue(KeyGroup, out var configuredKeys) || configuredKeys.Length == 0)
        {
            context.Result = CreateUnauthorizedResult(
                context,
                UnauthorizedErrorCode.AuthenticationRequired,
                $"No API keys are configured for key group '{KeyGroup}'.",
                KeyGroup);
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(options.HeaderName, out var providedKey)
            || string.IsNullOrWhiteSpace(providedKey))
        {
            context.Result = CreateUnauthorizedResult(
                context,
                UnauthorizedErrorCode.AuthenticationRequired,
                $"Header '{options.HeaderName}' is required for this endpoint.",
                options.HeaderName);
            return;
        }

        if (!configuredKeys.Any(configuredKey => SecureEquals(configuredKey, providedKey.ToString())))
        {
            context.Result = CreateUnauthorizedResult(
                context,
                UnauthorizedErrorCode.InvalidCredentials,
                "The supplied API key is invalid.",
                options.HeaderName);
            return;
        }

        await next();
    }

    private static UnauthorizedObjectResult CreateUnauthorizedResult(
        ActionExecutingContext context,
        string code,
        string message,
        string target)
    {
        var error = new AzureError(
            code,
            message,
            target,
            InnerError: new AzureInnerError(TraceId: context.HttpContext.TraceIdentifier));

        context.HttpContext.Response.Headers["x-ms-error-code"] = error.Code;
        return new UnauthorizedObjectResult(new AzureErrorResponse(error));
    }

    private static bool SecureEquals(string expected, string actual)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(actual);

        if (expectedBytes.Length != actualBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}