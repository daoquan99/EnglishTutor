using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

public sealed class NativeRabbitMqConsumerHostedService : BackgroundService
{
    private readonly RabbitMqConnectionProvider _connectionProvider;
    private readonly RabbitMqTopology _topology;
    private readonly RabbitMqPublisher _publisher;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IReadOnlyList<MessageConsumerRegistration> _registrations;
    private readonly ILogger<NativeRabbitMqConsumerHostedService> _logger;
    private readonly List<IChannel> _channels = [];
    private readonly object _channelsLock = new();

    public NativeRabbitMqConsumerHostedService(
        RabbitMqConnectionProvider connectionProvider,
        RabbitMqTopology topology,
        RabbitMqPublisher publisher,
        IServiceScopeFactory scopeFactory,
        IEnumerable<MessageConsumerRegistration> registrations,
        ILogger<NativeRabbitMqConsumerHostedService> logger)
    {
        _connectionProvider = connectionProvider;
        _topology = topology;
        _publisher = publisher;
        _scopeFactory = scopeFactory;
        _registrations = registrations.ToArray();
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await StartConsumersAsync(stoppingToken);
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "RabbitMQ consumer startup failed; retrying.");
                await DisposeChannelsAsync();
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task StartConsumersAsync(CancellationToken cancellationToken)
    {
        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        await using (var topologyChannel = await connection.CreateChannelAsync(cancellationToken: cancellationToken))
        {
            await _topology.DeclareAsync(topologyChannel, _registrations, cancellationToken);
        }

        foreach (var registration in _registrations)
        {
            for (var instance = 0; instance < registration.Descriptor.Concurrency; instance++)
            {
                var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
                lock (_channelsLock)
                {
                    _channels.Add(channel);
                }
                await channel.BasicQosAsync(
                    prefetchSize: 0,
                    prefetchCount: registration.Descriptor.PrefetchCount,
                    global: false,
                    cancellationToken: cancellationToken);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += (_, delivery) =>
                    HandleDeliveryAsync(channel, registration, delivery, cancellationToken);
                await channel.BasicConsumeAsync(
                    queue: registration.Descriptor.QueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: cancellationToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await DisposeChannelsAsync();
        await base.StopAsync(cancellationToken);
    }

    private async Task DisposeChannelsAsync()
    {
        IChannel[] channels;
        lock (_channelsLock)
        {
            channels = _channels.ToArray();
            _channels.Clear();
        }

        foreach (var channel in channels)
        {
            await channel.DisposeAsync();
        }
    }

    private async Task HandleDeliveryAsync(
        IChannel channel,
        MessageConsumerRegistration registration,
        BasicDeliverEventArgs delivery,
        CancellationToken cancellationToken)
    {
        var attempt = ReadAttempt(delivery.BasicProperties.Headers);
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var handler = (IRabbitMqMessageHandler)scope.ServiceProvider
                .GetRequiredService(registration.HandlerType);
            var context = CreateContext(delivery, attempt);
            await handler.HandleAsync(delivery.Body, context, cancellationToken);
            await channel.BasicAckAsync(
                deliveryTag: delivery.DeliveryTag,
                multiple: false,
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Consumer {ConsumerName} failed message {MessageId} at attempt {Attempt}.",
                registration.Descriptor.ConsumerName,
                delivery.BasicProperties.MessageId,
                attempt + 1);
            if (attempt + 1 < registration.Descriptor.MaxAttempts)
            {
                await _publisher.PublishRetryAsync(
                    queueName: registration.Descriptor.QueueName,
                    body: delivery.Body,
                    sourceProperties: delivery.BasicProperties,
                    attempt: attempt + 1,
                    cancellationToken: cancellationToken);
            }
            else
            {
                await _publisher.PublishDeadLetterAsync(
                    queueName: registration.Descriptor.QueueName,
                    body: delivery.Body,
                    sourceProperties: delivery.BasicProperties,
                    attempt: attempt + 1,
                    cancellationToken: cancellationToken);
            }

            await channel.BasicAckAsync(
                deliveryTag: delivery.DeliveryTag,
                multiple: false,
                cancellationToken: cancellationToken);
        }
    }

    private static MessageDeliveryContext CreateContext(
        BasicDeliverEventArgs delivery,
        int attempt)
    {
        var messageId = Guid.TryParse(delivery.BasicProperties.MessageId, out var parsed)
            ? parsed
            : throw new InvalidOperationException("RabbitMQ delivery has no valid message ID.");
        var headers = delivery.BasicProperties.Headers;
        return new MessageDeliveryContext(
            MessageId: messageId,
            ContractName: delivery.BasicProperties.Type ?? string.Empty,
            SchemaVersion: ReadHeader(headers, "schema-version") ?? string.Empty,
            CorrelationId: ParseGuid(delivery.BasicProperties.CorrelationId),
            CausationId: ParseGuid(ReadHeader(headers, "causation-id")),
            Attempt: attempt,
            ReceivedAtUtc: DateTimeOffset.UtcNow);
    }

    private static int ReadAttempt(IDictionary<string, object?>? headers)
    {
        if (headers is null || !headers.TryGetValue("retry-attempt", out var value))
        {
            return 0;
        }

        return value switch
        {
            int number => number,
            long number => checked((int)number),
            byte[] bytes when int.TryParse(Encoding.UTF8.GetString(bytes), out var number) => number,
            _ => 0
        };
    }

    private static string? ReadHeader(
        IDictionary<string, object?>? headers,
        string name)
    {
        if (headers is null || !headers.TryGetValue(name, out var value))
        {
            return null;
        }

        return value switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            string text => text,
            _ => value?.ToString()
        };
    }

    private static Guid? ParseGuid(string? value) =>
        Guid.TryParse(value, out var parsed) ? parsed : null;
}
