using MassTransit;
using EnglishTutor.IntegrationTests.Messaging.Events;

namespace EnglishTutor.IntegrationTests.Messaging.Consumers;

/// <summary>
/// A test-only consumer used strictly for verifying the messaging pipeline.
/// </summary>
public sealed class MessagingTestConsumer : IConsumer<MessagingTestEvent>
{
    public static readonly List<MessagingTestEvent> ConsumedEvents = [];

    public Task Consume(ConsumeContext<MessagingTestEvent> context)
    {
        lock (ConsumedEvents)
        {
            ConsumedEvents.Add(context.Message);
        }
        return Task.CompletedTask;
    }
}
