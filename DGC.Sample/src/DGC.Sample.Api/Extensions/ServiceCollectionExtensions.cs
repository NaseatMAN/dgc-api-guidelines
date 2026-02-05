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
}
