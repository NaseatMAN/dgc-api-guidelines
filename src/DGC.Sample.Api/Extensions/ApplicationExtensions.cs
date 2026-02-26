using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Interfaces.Notifications;
using DGC.Sample.Application.Services;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace DGC.Sample.Api.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<INotificationChannelSender, EmailNotificationChannelSender>();
        services.AddScoped<INotificationChannelSender, TelegramNotificationChannelSender>();
        services.AddScoped<INotificationSenderFactory, NotificationSenderFactory>();
        services.AddScoped<INotificationService, NotificationService>();

        services.AddValidatorsFromAssemblyContaining<IOrderService>();
        services.AddFluentValidationAutoValidation();

        return services;
    }
}
