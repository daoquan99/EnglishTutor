using EnglishTutor.Feedback.Infrastructure.Persistence;
using MassTransit;

namespace EnglishTutor.Feedback.Infrastructure.Consumers;

public sealed class PracticeSessionEndedConsumerDefinition : ConsumerDefinition<PracticeSessionEndedConsumer>
{
    public PracticeSessionEndedConsumerDefinition()
    {
        EndpointName = "feedback-practice-session-ended";
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<PracticeSessionEndedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<FeedbackDbContext>(context);
    }
}
