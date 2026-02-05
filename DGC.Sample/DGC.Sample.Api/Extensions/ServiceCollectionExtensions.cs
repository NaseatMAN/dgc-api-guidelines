using Asp.Versioning;
using DGC.Sample.Application.Abstractions.Interfaces;
using DGC.Sample.Application.Features.Orders.Handlers;
using DGC.Sample.Infrastructure.DependencyInjection;

namespace DGC.Sample.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDgcSampleServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddInfrastructure(configuration);

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
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "yyyy-MM-dd";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
