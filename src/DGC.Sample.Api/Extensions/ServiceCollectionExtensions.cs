using Asp.Versioning;
using DGC.Sample.Api.Filters;
using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Interfaces.Notifications;
using DGC.Sample.Application.Queue;
using DGC.Sample.Application.Queue.Messages;
using DGC.Sample.Application.Queue.Workers;
using DGC.Sample.Application.Queue.Workers.Handlers;
using DGC.Sample.Application.Services;
using DGC.Sample.Domain.Exceptions.Errors;
using DGC.Sample.Domain.Constants.ApiErrorConstants;
using DGC.Sample.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DGC.Sample.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDgcSampleServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<INotificationChannelSender, EmailNotificationChannelSender>();
        services.AddScoped<INotificationChannelSender, TelegramNotificationChannelSender>();
        services.AddScoped<INotificationSenderFactory, NotificationSenderFactory>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddInfrastructure(configuration);
        services.AddScoped<IMessageHandler<OrderCreatedMessage>, OrderCreatedMessageHandler>();
        services.AddHostedService<BackgroundOrderCreatedWorker>();

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddHostedService<BackgroundOrderCreatedRedisWorker>();
        }

        // Register FluentValidation
        services.AddValidatorsFromAssemblyContaining<IOrderService>();
        services.AddFluentValidationAutoValidation();

        return services;
    }

    public static IServiceCollection AddApiControllersWithAzureValidation(
        this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IProblemDetailsWriter, AzureProblemDetailsWriter>());
        services.AddScoped<IdempotencyFilter>();

        services
            .AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(entry => entry.Value?.Errors.Count > 0)
                        .SelectMany(entry => entry.Value!.Errors.Select(error =>
                            new AzureErrorDetail(
                                Code: $"{BadRequestErrorCode.InvalidModelError}.{entry.Key}",
                                Message: error.ErrorMessage,
                                Target: entry.Key)))
                        .ToList();

                    var azureError = new AzureError(
                        Code: BadRequestErrorCode.InvalidModelError,
                        Message: "One or more validation errors occurred.",
                        Details: errors,
                        InnerError: new AzureInnerError(TraceId: context.HttpContext.TraceIdentifier));

                    context.HttpContext.Response.Headers["x-ms-error-code"] = azureError.Code;

                    return new BadRequestObjectResult(new AzureErrorResponse(azureError));
                };
            });

        return services;
    }

    public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = false;
            options.ApiVersionReader = new QueryStringApiVersionReader("api-version");
            options.ReportApiVersions = true;
            options.UnsupportedApiVersionStatusCode = StatusCodes.Status400BadRequest;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "yyyy-MM-dd";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
