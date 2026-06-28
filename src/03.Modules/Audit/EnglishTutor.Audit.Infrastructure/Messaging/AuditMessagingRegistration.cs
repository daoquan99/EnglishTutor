using EnglishTutor.Audit.Infrastructure.Consumers;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.Identity.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Audit.Infrastructure.Messaging;

public static class AuditMessagingRegistration
{
    public static IServiceCollection AddAuditSecurityEventConsumers(
        this IServiceCollection services)
    {
        return services.AddNativeRabbitMqConsumer<IdentitySecurityEventConsumer>(
            new MessageConsumerDescriptor(
                ConsumerName: IdentitySecurityEventConsumer.ConsumerName,
                QueueName: IdentitySecurityEventConsumer.QueueName,
                MessageType: typeof(IdentitySecurityEventRecordedV1),
                PrefetchCount: 32,
                Concurrency: 4,
                MaxAttempts: 5,
                RetryDelay: TimeSpan.FromSeconds(10)));
    }
}
