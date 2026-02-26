using DGC.Sample.Application.Interfaces.Notifications;
using DGC.Sample.Application.Interfaces.Repositories;
using DGC.Sample.Application.Services;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace DGC.Sample.Api.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, OrderService>();
        services.AddScoped<IUserRepository, UserService>();
        services.AddScoped<INotificationChannelSender, EmailNotificationChannelSender>();
        services.AddScoped<INotificationChannelSender, TelegramNotificationChannelSender>();
        services.AddScoped<INotificationSenderFactory, NotificationSenderFactory>();
        services.AddScoped<INotificationService, NotificationService>();

        services.AddValidatorsFromAssemblyContaining<IOrderRepository>();
        services.AddFluentValidationAutoValidation();

        return services;
    }
}
