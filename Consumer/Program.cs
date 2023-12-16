using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

IHostBuilder builder = Host.CreateDefaultBuilder(args);

using IHost host = builder.Build();

// Application code should start here.

await host.RunAsync();