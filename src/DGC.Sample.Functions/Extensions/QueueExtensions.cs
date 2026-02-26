using DGC.Sample.Functions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DGC.Sample.Functions.Extensions;

public static class QueueExtensions
{
    public static IServiceCollection AddFunctionQueueOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<AzureFunctionsQueueOptions>()
            .Bind(configuration.GetSection(AzureFunctionsQueueOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<AzureFunctionsQueueOptions>, AzureFunctionsQueueOptionsValidator>();

        return services;
    }
}
