using Consumer.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Shared.Messages;

IHostBuilder builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(service =>
{
    service.AddMassTransit((config) =>
    {
        config.AddConsumer<ItemCreatedMessageConsume>();

        config.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(host: "amqps://welaavja:x6hkeE8trOMn48bGFhmy_q1hgpCYlZA-@crow.rmq.cloudamqp.com/welaavja");

            // ConfigureEndpoints should be the last method called after all settings and middleware components have been configured.
            cfg.ConfigureEndpoints(ctx);
        });
    });
});

using IHost host = builder.Build();

// Application code should start here.

await host.RunAsync();