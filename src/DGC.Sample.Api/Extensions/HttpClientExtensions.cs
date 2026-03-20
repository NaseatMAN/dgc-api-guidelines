using DGC.Sample.Application.Interfaces.ExternalServices;
using DGC.Sample.Infrastructure.ExternalServices.PublicApis;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using System.Net;

namespace DGC.Sample.Api.Extensions;

public static class HttpClientExtensions
{
    public static IServiceCollection AddExternalApiHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
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

        return services;
    }
}