using EnglishTutor.Feedback.Infrastructure.Persistence;
using MassTransit;

namespace EnglishTutor.Feedback.Infrastructure.Consumers;

public sealed class ExtractVocabularyRequestedConsumerDefinition : ConsumerDefinition<ExtractVocabularyRequestedConsumer>
{
    public ExtractVocabularyRequestedConsumerDefinition()
    {
        EndpointName = "feedback-extract-vocabulary-requested";
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ExtractVocabularyRequestedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<FeedbackDbContext>(context);
    }
}
