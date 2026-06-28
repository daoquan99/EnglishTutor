using EnglishTutor.BuildingBlocks.Contracts.Events;
using EnglishTutor.BuildingBlocks.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

public static class NativeMessagingServiceCollectionExtensions
{
    public static IServiceCollection AddNativeMessageContract<TMessage>(
        this IServiceCollection services,
        string contractName,
        string exchangeName,
        string routingKey)
        where TMessage : IntegrationEvent
    {
        services.AddSingleton(new MessageContractDescriptor(
            MessageType: typeof(TMessage),
            ContractName: contractName,
            ExchangeName: exchangeName,
            RoutingKey: routingKey));
        return services;
    }

    public static IServiceCollection AddNativeRabbitMqConsumer<THandler>(
        this IServiceCollection services,
        MessageConsumerDescriptor descriptor)
        where THandler : class, IRabbitMqMessageHandler
    {
        if (descriptor.MessageType != typeof(THandler).BaseType?.GenericTypeArguments.FirstOrDefault())
        {
            throw new InvalidOperationException(
                $"Consumer descriptor message type does not match {typeof(THandler).Name}.");
        }

        services.AddScoped<THandler>();
        services.AddSingleton(new MessageConsumerRegistration(typeof(THandler), descriptor));
        return services;
    }

    public static IServiceCollection AddNativeRabbitMqWorker(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddNativeRabbitMqConsumerRuntime(configuration);
        services.AddHostedService<NativeOutboxDispatcherHostedService>();
        return services;
    }

    public static IServiceCollection AddNativeRabbitMqConsumerRuntime(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<NativeMessagingOptions>()
            .Bind(configuration.GetSection(NativeMessagingOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                options => options.QueueType is "classic" or "quorum",
                "Messaging:NativeRabbitMq:QueueType must be classic or quorum.")
            .ValidateOnStart();
        services.TryAddSingleton<IMessageContractRegistry, MessageContractRegistry>();
        services.TryAddSingleton<RabbitMqConnectionProvider>();
        services.TryAddSingleton<RabbitMqTopology>();
        services.TryAddSingleton<RabbitMqPublisher>();
        services.AddHostedService<NativeRabbitMqConsumerHostedService>();
        return services;
    }

    public static IServiceCollection AddNativeMessagingRegistry(this IServiceCollection services)
    {
        services.TryAddSingleton<IMessageContractRegistry, MessageContractRegistry>();
        return services;
    }
}
