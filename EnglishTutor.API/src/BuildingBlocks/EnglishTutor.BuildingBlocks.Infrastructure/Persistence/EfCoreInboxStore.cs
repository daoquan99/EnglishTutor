using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Outbox;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Persistence;

public static class EfCoreInboxStore
{
    public static Task<bool> IsProcessedAsync(
        DbSet<InboxMessage> inboxMessages,
        Guid eventId,
        string handlerName,
        CancellationToken cancellationToken) =>
        inboxMessages.AnyAsync(
            message => message.EventId == eventId && message.HandlerName == handlerName,
            cancellationToken);

    public static void MarkProcessed(
        DbSet<InboxMessage> inboxMessages,
        IDateTimeProvider dateTimeProvider,
        Guid eventId,
        string eventType,
        string handlerName)
    {
        inboxMessages.Add(new InboxMessage
        {
            EventId = eventId,
            EventType = eventType,
            HandlerName = handlerName,
            ProcessedAtUtc = dateTimeProvider.UtcNow
        });
    }
}
