using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Interfaces.Notifications;
using DGC.Sample.Application.Interfaces.Persistence;
using DGC.Sample.Application.Interfaces.Repositories;
using DGC.Sample.Application.Interfaces.Services;
using DGC.Sample.Infrastructure.Caching;
using DGC.Sample.Infrastructure.ExternalServices.Notifications;
using DGC.Sample.Infrastructure.Identity;
using DGC.Sample.Infrastructure.Persistence.Data;
using DGC.Sample.Infrastructure.Persistence.Interceptors;
using DGC.Sample.Infrastructure.Persistence.Repositories;
using DGC.Sample.Infrastructure.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DGC.Sample.Functions.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddFunctionInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var defaultConnection =
            configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=dgc_sample;Username=postgres;Password=password";

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(defaultConnection)
                .AddInterceptors(new SoftDeleteInterceptor(), new AuditInterceptor()));

        services.AddHttpContextAccessor();
        services.AddScoped<ITenantAccessor, HttpTenantAccessor>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddHybridCache();
        services.AddScoped<IIdempotencyService, HybridCacheIdempotencyService>();

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
