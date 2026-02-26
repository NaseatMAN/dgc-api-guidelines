using DGC.Sample.Functions.Extensions;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder();
host.ConfigureEnvironmentVariables();
host.ConfigureFunctionWorker();
host.ConfigureFunctionServices();
await host.Build().RunAsync();