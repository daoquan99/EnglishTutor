using MassTransit;
using EnglishTutor.Realtime.Infrastructure.Consumers;

namespace EnglishTutor.Realtime.Infrastructure.Messaging;

public static class RealtimeMessagingRegistration
{
    public static void AddRealtimeConsumers(this IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<FeedbackReadyIntegrationEventConsumer, FeedbackReadyIntegrationEventConsumerDefinition>();
    }
}
