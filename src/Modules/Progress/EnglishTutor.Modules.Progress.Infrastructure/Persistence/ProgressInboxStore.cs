using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Progress.Infrastructure.Persistence;

public sealed class ProgressInboxStore(ProgressDbContext dbContext) : IProgressInboxStore
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
