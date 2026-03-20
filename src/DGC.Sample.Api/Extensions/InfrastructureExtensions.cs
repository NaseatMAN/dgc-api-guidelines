using DGC.Sample.Application.Interfaces.Persistence;
using DGC.Sample.Application.Interfaces.Repositories;
using DGC.Sample.Application.Interfaces.ExternalServices;
using DGC.Sample.Infrastructure.Caching;
using DGC.Sample.Infrastructure.ExternalServices.PublicApis;
using DGC.Sample.Infrastructure.Persistence.Data;
using DGC.Sample.Infrastructure.Persistence.Interceptors;
using DGC.Sample.Infrastructure.Persistence.Repositories.Purchases;
using DGC.Sample.Infrastructure.Persistence.Repositories.UserMgmt;
using DGC.Sample.Infrastructure.Persistence.UnitOfWorks;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.EntityFrameworkCore;
using Polly;
using System.Net;

namespace DGC.Sample.Api.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddApiInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var defaultConnection =
            configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(defaultConnection)
                .AddInterceptors(new SoftDeleteInterceptor(), new AuditInterceptor()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services
            .AddOptions<JsonPlaceholderClientSettings>()
            .Bind(configuration.GetSection(JsonPlaceholderClientSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddHttpClient(JsonPlaceholderUserClient.ClientName, (serviceProvider, httpClient) =>
            {
                var settings = serviceProvider
                    .GetRequiredService<Microsoft.Extensions.Options.IOptions<JsonPlaceholderClientSettings>>()
                    .Value;

                httpClient.BaseAddress = new Uri(settings.BaseUrl);
                httpClient.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("DGC.Sample.Api/1.0");
            })
            .AddResilienceHandler("jsonplaceholder-resilience", static (builder, context) =>
            {
                var settings = context.ServiceProvider
                    .GetRequiredService<Microsoft.Extensions.Options.IOptions<JsonPlaceholderClientSettings>>()
                    .Value;

                var shouldHandleTransient = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(response =>
                        (int)response.StatusCode >= 500 ||
                        response.StatusCode == HttpStatusCode.RequestTimeout ||
                        (int)response.StatusCode == 429);

                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = settings.Retry.MaxRetryAttempts,
                    Delay = TimeSpan.FromMilliseconds(settings.Retry.BaseDelayMs),
                    MaxDelay = TimeSpan.FromMilliseconds(settings.Retry.MaxDelayMs),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = shouldHandleTransient
                });

                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = settings.CircuitBreaker.FailureRatio,
                    MinimumThroughput = settings.CircuitBreaker.MinimumThroughput,
                    SamplingDuration = TimeSpan.FromSeconds(settings.CircuitBreaker.SamplingDurationSeconds),
                    BreakDuration = TimeSpan.FromSeconds(settings.CircuitBreaker.BreakDurationSeconds),
                    ShouldHandle = shouldHandleTransient
                });
            });

        services.AddScoped<IPublicUserLookupClient, JsonPlaceholderUserClient>();

        services.AddHybridCache();
        services.AddScoped<IIdempotencyService, HybridCacheIdempotencyService>();

        return services;
    }

    public static async Task<WebApplication> ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        return app;
    }
}
