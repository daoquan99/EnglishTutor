using System.Text.Json;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

public abstract class RabbitMqMessageHandler<TMessage> : IRabbitMqMessageHandler
    where TMessage : class
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public abstract MessageConsumerDescriptor Descriptor { get; }

    public async Task HandleAsync(
        ReadOnlyMemory<byte> body,
        MessageDeliveryContext context,
        CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize<TMessage>(body.Span, SerializerOptions)
            ?? throw new JsonException($"Unable to deserialize {typeof(TMessage).Name}.");
        await HandleAsync(message, context, cancellationToken);
    }

    protected abstract Task HandleAsync(
        TMessage message,
        MessageDeliveryContext context,
        CancellationToken cancellationToken);
}
