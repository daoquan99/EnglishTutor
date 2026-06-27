using MassTransit;

namespace EnglishTutor.Realtime.Infrastructure.Consumers;

public sealed class FeedbackReadyIntegrationEventConsumerDefinition : ConsumerDefinition<FeedbackReadyIntegrationEventConsumer>
{
    public FeedbackReadyIntegrationEventConsumerDefinition()
    {
        EndpointName = "realtime-feedback-ready";
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<FeedbackReadyIntegrationEventConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        // No DB transactional outbox needed as this consumer has no local DB side effects.
    }
}
