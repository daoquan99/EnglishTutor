using EnglishTutor.Feedback.Infrastructure.Persistence;
using MassTransit;

namespace EnglishTutor.Feedback.Infrastructure.Consumers;

public sealed class GenerateSessionFeedbackRequestedConsumerDefinition : ConsumerDefinition<GenerateSessionFeedbackRequestedConsumer>
{
    public GenerateSessionFeedbackRequestedConsumerDefinition()
    {
        EndpointName = "feedback-generate-session-feedback-requested";
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<GenerateSessionFeedbackRequestedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<FeedbackDbContext>(context);
    }
}
