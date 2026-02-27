using DGC.Sample.Application.Queue;
using DGC.Sample.Application.Queue.Messages;
using DGC.Sample.Application.Queue.Workers.Handlers;
using DGC.Sample.Application.Interfaces.Services;
using DGC.Sample.Application.Services;
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
