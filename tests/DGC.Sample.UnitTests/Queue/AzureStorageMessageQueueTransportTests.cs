using DGC.Sample.Application.Queue;
using DGC.Sample.Application.Queue.Exceptions;
using DGC.Sample.Infrastructure.DependencyInjection;
using DGC.Sample.Infrastructure.Queue;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DGC.Sample.UnitTests.Queue;

public sealed class AzureStorageMessageQueueTransportTests
{
    [Fact]
    public async Task DequeueAsync_ShouldThrowNotSupportedException()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["AzureWebJobsStorage"] = "UseDevelopmentStorage=true",
            ["AzureFunctions:QueueName"] = "orders"
        });

        var transport = new AzureStorageMessageQueueTransport<string>(configuration);

        var act = () => transport.DequeueAsync(0, CancellationToken.None);

        await act.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public void AddQueueServices_WhenAzureConfigPresent_ShouldResolveAzureTransport()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Queue:DefaultTransport"] = "AzureQueue",
            ["AzureWebJobsStorage"] = "UseDevelopmentStorage=true",
            ["Queue:Azure:QueueName"] = "orders"
        });

        var services = new ServiceCollection();
        services.AddQueueServices(configuration);

        using var provider = services.BuildServiceProvider();
        var resolver = provider.GetRequiredService<ITransportResolver<string>>();

        var resolved = resolver.Resolve(QueueTransport.AzureQueue);

        resolved.Should().BeOfType<AzureStorageMessageQueueTransport<string>>();
    }

    [Fact]
    public void AddQueueServices_WhenAzureDefaultConfiguredWithoutRequiredSettings_ShouldThrow()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Queue:DefaultTransport"] = "AzureQueue"
        });

        var services = new ServiceCollection();

        var act = () => services.AddQueueServices(configuration);

        act.Should().Throw<TransportInitializationException>();
    }

    private static IConfiguration BuildConfiguration(IReadOnlyDictionary<string, string?> data)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
    }
}