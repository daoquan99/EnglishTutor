using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.Mistakes.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Mistakes.Infrastructure.Persistence;

public sealed class MistakesInboxStore(MistakesDbContext dbContext) : IMistakesInboxStore
{
    public Task<bool> IsProcessedAsync(Guid eventId, string handlerName, CancellationToken cancellationToken) =>
        dbContext.InboxMessages.AnyAsync(
            message => message.EventId == eventId && message.HandlerName == handlerName,
            cancellationToken);

    public Task MarkProcessedAsync(Guid eventId, string eventType, string handlerName, CancellationToken cancellationToken)
    {
        dbContext.InboxMessages.Add(new InboxMessage
        {
            EventId = eventId,
            EventType = eventType,
            HandlerName = handlerName
        });

        return Task.CompletedTask;
    }
}
