using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using EnglishTutor.BuildingBlocks.Infrastructure.Options;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// Configures MassTransit on the API utilizing the In-Memory transport stub.
    /// This allows writing to EF outbox tables without establishing a network connection to RabbitMQ.
    /// </summary>
    public static IServiceCollection AddApiOutboxMessaging(
        this IServiceCollection services,
        Action<IBusRegistrationConfigurator> configureOutboxes)
    {
        services.AddMassTransit(x =>
        {
            configureOutboxes(x);

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    /// <summary>
    /// Configures MassTransit on the Worker connecting to the RabbitMQ broker
    /// and hosting the outbox delivery background services and queue consumers.
    /// </summary>
    public static IServiceCollection AddWorkerBrokerMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator> configureWorker)
    {
        services.AddMassTransit(x =>
        {
            configureWorker(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                cfg.Host(options.HostName, (ushort)options.Port, options.VirtualHost, h =>
                {
                    h.Username(options.UserName);
                    h.Password(options.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
