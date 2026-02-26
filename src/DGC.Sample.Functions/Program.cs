using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Interfaces.Repositories;
using DGC.Sample.Application.Queue;
using DGC.Sample.Application.Queue.Messages;
using DGC.Sample.Application.Queue.Workers.Handlers;
using DGC.Sample.Application.Services;
using DGC.Sample.Functions.Configuration;
using DGC.Sample.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var host = new HostBuilder()
    .ConfigureAppConfiguration((context, configurationBuilder) =>
    {
        configurationBuilder
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        if (context.HostingEnvironment.IsDevelopment())
        {
            configurationBuilder.AddUserSecrets<Program>(optional: true);
        }
    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services
            .AddOptions<AzureFunctionsQueueOptions>()
            .Bind(context.Configuration.GetSection(AzureFunctionsQueueOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<AzureFunctionsQueueOptions>, AzureFunctionsQueueOptionsValidator>();

        services.AddInfrastructure(context.Configuration);
        services.AddScoped<DGC.Sample.Application.Interfaces.Repositories.IOrderRepository, OrderService>();
        services.AddScoped<IMessageHandler<OrderCreatedMessage>, OrderCreatedMessageHandler>();
    })
    .Build();

host.Run();
