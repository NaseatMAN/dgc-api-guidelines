using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DGC.Sample.Domain.Constants.ApiErrorConstants;
using DGC.Sample.Domain.Exceptions.Errors;

namespace DGC.Sample.Api.Filters;

public sealed class AsyncValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

            if (context.HttpContext.RequestServices.GetService(validatorType) is IValidator validator)
            {
                var validationResult = await validator.ValidateAsync(new ValidationContext<object>(argument), context.HttpContext.RequestAborted);

                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .Select(error => new AzureErrorDetail(
                            Code: $"{BadRequestErrorCode.InvalidModelError}.{error.PropertyName}",
                            Message: error.ErrorMessage,
                            Target: error.PropertyName))
                        .ToList();

                    var azureError = new AzureError(
                        Code: BadRequestErrorCode.InvalidModelError,
                        Message: "One or more validation errors occurred.",
                        Details: errors,
                        InnerError: new AzureInnerError(TraceId: context.HttpContext.TraceIdentifier));

                    context.HttpContext.Response.Headers["x-ms-error-code"] = azureError.Code;
                    context.Result = new BadRequestObjectResult(new AzureErrorResponse(azureError));
                    return;
                }
            }
        }

        await next();
    }
}
