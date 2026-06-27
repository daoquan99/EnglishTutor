using EnglishTutor.BuildingBlocks.Infrastructure.Options;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddApiOutboxMessaging(
        this IServiceCollection services,
        Action<IBusRegistrationConfigurator> configureOutboxes)
    {
        services.AddMassTransit(x =>
        {
            configureOutboxes(x);

            x.UsingInMemory((context, cfg) =>
            {
                cfg.UseConsumeFilter(typeof(EventEnvelopeContextFilter<>), context);
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    public static IServiceCollection AddApiOutboxMessaging<TBus>(
        this IServiceCollection services,
        Action<IBusRegistrationConfigurator> configureDefaultBus,
        Action<IBusRegistrationConfigurator<TBus>> configureModuleBus)
        where TBus : class, IBus
    {
        services.AddApiOutboxMessaging(configureDefaultBus);

        services.AddMassTransit<TBus>(x =>
        {
            configureModuleBus(x);

            x.UsingInMemory((context, cfg) =>
            {
                cfg.UseConsumeFilter(typeof(EventEnvelopeContextFilter<>), context);
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

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

                cfg.UseConsumeFilter(typeof(EventEnvelopeContextFilter<>), context);
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    public static IServiceCollection AddWorkerBrokerMessaging<TBus>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator> configureDefaultBus,
        Action<IBusRegistrationConfigurator<TBus>> configureModuleBus)
        where TBus : class, IBus
    {
        services.AddWorkerBrokerMessaging(configuration, configureDefaultBus);

        services.AddMassTransit<TBus>(x =>
        {
            configureModuleBus(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                cfg.Host(options.HostName, (ushort)options.Port, options.VirtualHost, h =>
                {
                    h.Username(options.UserName);
                    h.Password(options.Password);
                });

                cfg.UseConsumeFilter(typeof(EventEnvelopeContextFilter<>), context);
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
