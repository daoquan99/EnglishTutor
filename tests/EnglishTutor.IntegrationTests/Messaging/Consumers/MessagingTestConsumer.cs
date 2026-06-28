using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.IntegrationTests.Messaging.Events;

namespace EnglishTutor.IntegrationTests.Messaging.Consumers;

public sealed class MessagingTestConsumer : RabbitMqMessageHandler<MessagingTestEvent>
{
    public static readonly List<MessagingTestEvent> ConsumedEvents = [];

    public override MessageConsumerDescriptor Descriptor { get; } = new(
        "tests.messaging.v1",
        "english.tests.messaging.v1",
        typeof(MessagingTestEvent),
        PrefetchCount: 1,
        Concurrency: 1);

    protected override Task HandleAsync(
        MessagingTestEvent message,
        MessageDeliveryContext context,
        CancellationToken cancellationToken)
    {
        lock (ConsumedEvents)
        {
            ConsumedEvents.Add(message);
        }

        return Task.CompletedTask;
    }
}
