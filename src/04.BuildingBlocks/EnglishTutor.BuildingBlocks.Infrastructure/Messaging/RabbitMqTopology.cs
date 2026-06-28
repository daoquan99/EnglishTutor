using EnglishTutor.BuildingBlocks.Infrastructure.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

public sealed class RabbitMqTopology
{
    private readonly IMessageContractRegistry _registry;
    private readonly NativeMessagingOptions _options;

    public RabbitMqTopology(
        IMessageContractRegistry registry,
        IOptions<NativeMessagingOptions> options)
    {
        _registry = registry;
        _options = options.Value;
    }

    public async Task DeclareAsync(
        IChannel channel,
        IEnumerable<MessageConsumerRegistration> registrations,
        CancellationToken cancellationToken)
    {
        var alternate = new Dictionary<string, object?>
        {
            ["alternate-exchange"] = MessageTopologyNames.UnroutableExchange
        };

        await channel.ExchangeDeclareAsync(
            exchange: MessageTopologyNames.IntegrationExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: alternate,
            cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(
            exchange: MessageTopologyNames.CommandExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: alternate,
            cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(
            exchange: MessageTopologyNames.RetryExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(
            exchange: MessageTopologyNames.DeadLetterExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(
            exchange: MessageTopologyNames.UnroutableExchange,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(
            queue: MessageTopologyNames.UnroutableQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);
        await channel.QueueBindAsync(
            queue: MessageTopologyNames.UnroutableQueue,
            exchange: MessageTopologyNames.UnroutableExchange,
            routingKey: string.Empty,
            cancellationToken: cancellationToken);

        foreach (var registration in registrations)
        {
            var consumer = registration.Descriptor;
            var contract = _registry.Get(consumer.MessageType);
            var queueArguments = CreateQueueArguments();
            await channel.QueueDeclareAsync(
                queue: consumer.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArguments,
                cancellationToken: cancellationToken);
            await channel.QueueBindAsync(
                queue: consumer.QueueName,
                exchange: contract.ExchangeName,
                routingKey: contract.RoutingKey,
                cancellationToken: cancellationToken);

            var retryQueue = $"{consumer.QueueName}.retry";
            var retryArguments = CreateQueueArguments();
            retryArguments["x-message-ttl"] = (int)consumer.EffectiveRetryDelay.TotalMilliseconds;
            retryArguments["x-dead-letter-exchange"] = contract.ExchangeName;
            retryArguments["x-dead-letter-routing-key"] = contract.RoutingKey;
            await channel.QueueDeclareAsync(
                queue: retryQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: retryArguments,
                cancellationToken: cancellationToken);
            await channel.QueueBindAsync(
                queue: retryQueue,
                exchange: MessageTopologyNames.RetryExchange,
                routingKey: consumer.QueueName,
                cancellationToken: cancellationToken);

            var deadLetterQueue = $"{consumer.QueueName}.dlq";
            await channel.QueueDeclareAsync(
                queue: deadLetterQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: CreateQueueArguments(),
                cancellationToken: cancellationToken);
            await channel.QueueBindAsync(
                queue: deadLetterQueue,
                exchange: MessageTopologyNames.DeadLetterExchange,
                routingKey: consumer.QueueName,
                cancellationToken: cancellationToken);
        }
    }

    private Dictionary<string, object?> CreateQueueArguments()
    {
        var arguments = new Dictionary<string, object?>();
        if (string.Equals(_options.QueueType, "quorum", StringComparison.OrdinalIgnoreCase))
        {
            arguments["x-queue-type"] = "quorum";
        }

        return arguments;
    }
}
