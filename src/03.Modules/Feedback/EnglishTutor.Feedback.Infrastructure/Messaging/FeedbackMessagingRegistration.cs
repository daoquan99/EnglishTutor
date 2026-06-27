using EnglishTutor.Feedback.Infrastructure.Consumers;
using MassTransit;

namespace EnglishTutor.Feedback.Infrastructure.Messaging;

public static class FeedbackMessagingRegistration
{
    public static void AddFeedbackConsumers(this IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<PracticeSessionEndedConsumer, PracticeSessionEndedConsumerDefinition>();
        configurator.AddConsumer<GenerateSessionFeedbackRequestedConsumer, GenerateSessionFeedbackRequestedConsumerDefinition>();
        configurator.AddConsumer<ExtractVocabularyRequestedConsumer, ExtractVocabularyRequestedConsumerDefinition>();
    }

    public static void AddFeedbackConsumers<TBus>(this IBusRegistrationConfigurator<TBus> configurator)
        where TBus : class, IBus
    {
        configurator.AddConsumer<PracticeSessionEndedConsumer, PracticeSessionEndedConsumerDefinition>();
        configurator.AddConsumer<GenerateSessionFeedbackRequestedConsumer, GenerateSessionFeedbackRequestedConsumerDefinition>();
        configurator.AddConsumer<ExtractVocabularyRequestedConsumer, ExtractVocabularyRequestedConsumerDefinition>();
    }
}
