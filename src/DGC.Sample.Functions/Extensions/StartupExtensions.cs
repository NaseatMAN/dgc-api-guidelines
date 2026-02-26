using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DGC.Sample.Functions.Extensions;

public static class StartupExtensions
{
    public static IHostBuilder ConfigureEnvironmentVariables(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureAppConfiguration(ConfigureAppConfiguration);
    }

    public static IHostBuilder ConfigureFunctionWorker(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureFunctionsWorkerDefaults();
    }

    public static IHostBuilder ConfigureFunctionServices(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddFunctionQueueOptions(context.Configuration);
            services.AddFunctionInfrastructure(context.Configuration);
            services.AddFunctionApplicationServices();
        });
    }

    private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        if (context.HostingEnvironment.IsDevelopment())
        {
            configurationBuilder.AddUserSecrets<Program>(optional: true);
        }
    }
}
