namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

public interface IRabbitMqMessageHandler
{
    MessageConsumerDescriptor Descriptor { get; }

    Task HandleAsync(
        ReadOnlyMemory<byte> body,
        MessageDeliveryContext context,
        CancellationToken cancellationToken);
}
