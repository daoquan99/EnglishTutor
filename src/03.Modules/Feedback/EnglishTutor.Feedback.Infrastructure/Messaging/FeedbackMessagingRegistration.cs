using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.Feedback.Contracts.Events;
using EnglishTutor.Feedback.Infrastructure.Consumers;
using EnglishTutor.Practice.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Feedback.Infrastructure.Messaging;

public static class FeedbackMessagingRegistration
{
    public static IServiceCollection AddFeedbackConsumers(this IServiceCollection services)
    {
        services.AddNativeRabbitMqConsumer<PracticeSessionEndedConsumer>(new(
            PracticeSessionEndedConsumer.ConsumerName,
            PracticeSessionEndedConsumer.QueueName,
            typeof(PracticeSessionEndedIntegrationEventV1),
            PrefetchCount: 16,
            Concurrency: 4));
        services.AddNativeRabbitMqConsumer<GenerateSessionFeedbackRequestedConsumer>(new(
            GenerateSessionFeedbackRequestedConsumer.ConsumerName,
            GenerateSessionFeedbackRequestedConsumer.QueueName,
            typeof(GenerateSessionFeedbackRequestedV1),
            PrefetchCount: 2,
            Concurrency: 2,
            MaxAttempts: 5,
            RetryDelay: TimeSpan.FromSeconds(30)));
        services.AddNativeRabbitMqConsumer<ExtractVocabularyRequestedConsumer>(new(
            ExtractVocabularyRequestedConsumer.ConsumerName,
            ExtractVocabularyRequestedConsumer.QueueName,
            typeof(ExtractVocabularyRequestedV1),
            PrefetchCount: 16,
            Concurrency: 2));
        return services;
    }
}
