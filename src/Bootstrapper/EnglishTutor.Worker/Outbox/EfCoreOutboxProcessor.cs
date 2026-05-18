using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.Auth.Infrastructure.Persistence;
using EnglishTutor.Modules.Mistakes.Infrastructure.Persistence;
using EnglishTutor.Modules.Speaking.Infrastructure.Persistence;
using EnglishTutor.Modules.Users.Infrastructure.Persistence;
using EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Worker.Outbox;

public sealed class EfCoreOutboxProcessor(
    AuthDbContext authDbContext,
    UsersDbContext usersDbContext,
    VocabularyDbContext vocabularyDbContext,
    SpeakingDbContext speakingDbContext,
    MistakesDbContext mistakesDbContext,
    MessagingDbContext messagingDbContext,
    IEventBus eventBus,
    JsonSerializerService serializer,
    ILogger<EfCoreOutboxProcessor> logger) : IOutboxProcessor
{
    private readonly string _workerId = $"{Environment.MachineName}-{Guid.NewGuid():N}";

    public async Task ProcessPendingMessagesAsync(CancellationToken ct = default)
    {
        await ProcessStoreAsync("auth", authDbContext.OutboxMessages, authDbContext, ct);
        await ProcessStoreAsync("users", usersDbContext.OutboxMessages, usersDbContext, ct);
        await ProcessStoreAsync("vocabulary", vocabularyDbContext.OutboxMessages, vocabularyDbContext, ct);
        await ProcessStoreAsync("speaking", speakingDbContext.OutboxMessages, speakingDbContext, ct);
        await ProcessStoreAsync("mistakes", mistakesDbContext.OutboxMessages, mistakesDbContext, ct);
    }

    private async Task ProcessStoreAsync(string moduleName, DbSet<OutboxMessage> outboxMessages, DbContext dbContext, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var messages = await outboxMessages
            .Where(message =>
                (message.Status == OutboxMessageStatus.Pending || message.Status == OutboxMessageStatus.Failed) &&
                (message.NextRetryAtUtc == null || message.NextRetryAtUtc <= now) &&
                (message.LockedUntilUtc == null || message.LockedUntilUtc <= now))
            .OrderBy(message => message.CreatedAtUtc)
            .Take(20)
            .ToListAsync(ct);

        foreach (var message in messages)
        {
            message.Status = OutboxMessageStatus.Processing;
            message.LockedBy = _workerId;
            message.LockedUntilUtc = DateTime.UtcNow.AddMinutes(1);
            await dbContext.SaveChangesAsync(ct);

            try
            {
                var eventType = Type.GetType(message.EventType, throwOnError: true)!;
                var integrationEvent = (IIntegrationEvent)serializer.Deserialize(message.Payload, eventType)!;

                await eventBus.PublishAsync(integrationEvent, ct);

                message.Status = OutboxMessageStatus.Processed;
                message.ProcessedAtUtc = DateTime.UtcNow;
                message.LockedBy = null;
                message.LockedUntilUtc = null;
                await dbContext.SaveChangesAsync(ct);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed processing outbox message {MessageId} from {Module}", message.Id, moduleName);

                message.RetryCount++;
                message.LastError = exception.Message;
                message.Status = OutboxMessageStatus.Failed;
                message.LockedBy = null;
                message.LockedUntilUtc = null;

                if (message.RetryCount >= message.MaxRetryCount)
                {
                    message.NextRetryAtUtc = DateTime.MaxValue;
                    messagingDbContext.DeadLetterMessages.Add(new DeadLetterMessage
                    {
                        EventId = message.EventId,
                        EventType = message.EventType,
                        Payload = message.Payload,
                        SourceModule = message.SourceModule,
                        RetryCount = message.RetryCount,
                        LastError = exception.Message,
                        StackTrace = exception.ToString()
                    });
                }
                else
                {
                    message.NextRetryAtUtc = DateTime.UtcNow.AddSeconds(Math.Pow(2, message.RetryCount) * 10);
                }

                await dbContext.SaveChangesAsync(ct);
                await messagingDbContext.SaveChangesAsync(ct);
            }
        }
    }
}
