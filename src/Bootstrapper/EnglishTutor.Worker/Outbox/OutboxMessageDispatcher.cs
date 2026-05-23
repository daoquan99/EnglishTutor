using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.BuildingBlocks.Outbox.Processing;
using Microsoft.Extensions.Options;

namespace EnglishTutor.Worker.Outbox;

public sealed class OutboxMessageDispatcher(
    IEventBus eventBus,
    JsonSerializerService serializer,
    ILogger<OutboxMessageDispatcher> logger,
    IDateTimeProvider dateTimeProvider,
    IOptions<OutboxOptions> options)
{
    private readonly OutboxOptions _options = options.Value;

    public async Task<DeadLetterMessage?> DispatchAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var eventType = Type.GetType(message.EventType, throwOnError: true)!;
            var integrationEvent = (IIntegrationEvent)serializer.Deserialize(message.Payload, eventType)!;

            await eventBus.PublishAsync(integrationEvent, cancellationToken);

            message.Status = OutboxMessageStatus.Processed;
            message.ProcessedAtUtc = dateTimeProvider.UtcNow;
            message.LockedBy = null;
            message.LockedUntilUtc = null;
            message.NextRetryAtUtc = null;

            return null;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed dispatching outbox message {MessageId}", message.Id);

            message.RetryCount++;
            message.LastError = exception.Message;
            message.LockedBy = null;
            message.LockedUntilUtc = null;

            var maxRetry = message.MaxRetryCount > 0 ? message.MaxRetryCount : _options.MaxRetryCount;
            if (message.RetryCount >= maxRetry)
            {
                message.Status = OutboxMessageStatus.DeadLettered;
                message.NextRetryAtUtc = null;
                return new DeadLetterMessage
                {
                    EventId = message.EventId,
                    EventType = message.EventType,
                    Payload = message.Payload,
                    SourceModule = message.SourceModule,
                    FailedAtUtc = dateTimeProvider.UtcNow,
                    RetryCount = message.RetryCount,
                    LastError = exception.Message,
                    StackTrace = exception.ToString()
                };
            }

            message.Status = OutboxMessageStatus.Pending;
            message.NextRetryAtUtc = RetryPolicy.CalculateNextRetry(message.RetryCount, dateTimeProvider.UtcNow);
            return null;
        }
    }
}
