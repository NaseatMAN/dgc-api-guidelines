using DGC.Sample.Application.Dtos.Queue;
using DGC.Sample.Application.Interfaces.Queue;
using DGC.Sample.Application.Interfaces.Services;
using DGC.Sample.Application.Services;
using DGC.Sample.Application.Services.Queue.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace DGC.Sample.Functions.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddFunctionApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IMessageHandler<OrderCreatedMessage>, OrderCreatedMessageHandler>();

        return services;
    }
}
