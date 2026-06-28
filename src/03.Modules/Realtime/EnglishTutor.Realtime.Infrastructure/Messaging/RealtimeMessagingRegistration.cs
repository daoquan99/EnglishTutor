using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.Feedback.Contracts.Events;
using EnglishTutor.Realtime.Infrastructure.Consumers;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Realtime.Infrastructure.Messaging;

public static class RealtimeMessagingRegistration
{
    public static IServiceCollection AddRealtimeConsumers(this IServiceCollection services)
    {
        return services.AddNativeRabbitMqConsumer<FeedbackReadyIntegrationEventConsumer>(new(
            FeedbackReadyIntegrationEventConsumer.ConsumerName,
            FeedbackReadyIntegrationEventConsumer.QueueName,
            typeof(FeedbackReadyIntegrationEventV1),
            PrefetchCount: 32,
            Concurrency: 4,
            MaxAttempts: 5,
            RetryDelay: TimeSpan.FromSeconds(10)));
    }
}
