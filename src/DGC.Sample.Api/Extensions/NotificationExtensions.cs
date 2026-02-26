using DGC.Sample.Application.Interfaces.Notifications;
using DGC.Sample.Infrastructure.ExternalServices.Notifications;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DGC.Sample.Api.Extensions;

public static class NotificationExtensions
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var emailSettings = configuration
            .GetSection(EmailNotificationSettings.SectionName)
            .Get<EmailNotificationSettings>()
            ?? new EmailNotificationSettings();

        var telegramSettings = configuration
            .GetSection(TelegramNotificationSettings.SectionName)
            .Get<TelegramNotificationSettings>()
            ?? new TelegramNotificationSettings();

        services.TryAddSingleton(emailSettings);
        services.TryAddSingleton(telegramSettings);
        services.TryAddScoped<IEmailSender, SmtpEmailSender>();
        services.TryAddScoped<ITelegramSender, TelegramSender>();

        return services;
    }
}
