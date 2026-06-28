using System.Text;
using EnglishTutor.BuildingBlocks.Infrastructure.Outbox;
using RabbitMQ.Client;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

public sealed class RabbitMqPublisher : IAsyncDisposable
{
    private readonly RabbitMqConnectionProvider _connectionProvider;
    private readonly SemaphoreSlim _publishGate = new(1, 1);
    private IChannel? _channel;

    public RabbitMqPublisher(RabbitMqConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var properties = new BasicProperties
        {
            Persistent = true,
            MessageId = message.Id.ToString("D"),
            Type = message.ContractName,
            CorrelationId = message.CorrelationId?.ToString("D"),
            Timestamp = new AmqpTimestamp(message.OccurredAtUtc.ToUnixTimeSeconds()),
            Headers = new Dictionary<string, object?>
            {
                ["schema-version"] = message.SchemaVersion,
                ["causation-id"] = message.CausationId?.ToString("D") ?? string.Empty
            }
        };

        await PublishAsync(
            message.ExchangeName,
            message.RoutingKey,
            Encoding.UTF8.GetBytes(message.PayloadJson),
            properties,
            cancellationToken);
    }

    public async Task PublishRetryAsync(
        string queueName,
        ReadOnlyMemory<byte> body,
        IReadOnlyBasicProperties sourceProperties,
        int attempt,
        CancellationToken cancellationToken)
    {
        var properties = CopyProperties(sourceProperties, attempt);
        await PublishAsync(
            MessageTopologyNames.RetryExchange,
            queueName,
            body,
            properties,
            cancellationToken);
    }

    public async Task PublishDeadLetterAsync(
        string queueName,
        ReadOnlyMemory<byte> body,
        IReadOnlyBasicProperties sourceProperties,
        int attempt,
        CancellationToken cancellationToken)
    {
        var properties = CopyProperties(sourceProperties, attempt);
        await PublishAsync(
            MessageTopologyNames.DeadLetterExchange,
            queueName,
            body,
            properties,
            cancellationToken);
    }

    private async Task PublishAsync(
        string exchange,
        string routingKey,
        ReadOnlyMemory<byte> body,
        BasicProperties properties,
        CancellationToken cancellationToken)
    {
        await _publishGate.WaitAsync(cancellationToken);
        try
        {
            var channel = await GetChannelAsync(cancellationToken);
            await channel.BasicPublishAsync(
                exchange,
                routingKey,
                mandatory: true,
                properties,
                body,
                cancellationToken);
        }
        finally
        {
            _publishGate.Release();
        }
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        _channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true),
            cancellationToken);
        return _channel;
    }

    private static BasicProperties CopyProperties(
        IReadOnlyBasicProperties source,
        int attempt)
    {
        var headers = source.Headers is null
            ? new Dictionary<string, object?>()
            : new Dictionary<string, object?>(source.Headers);
        headers["retry-attempt"] = attempt;
        return new BasicProperties
        {
            Persistent = true,
            MessageId = source.MessageId,
            Type = source.Type,
            CorrelationId = source.CorrelationId,
            Timestamp = source.Timestamp,
            Headers = headers
        };
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        _publishGate.Dispose();
    }
}
