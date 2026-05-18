using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.Users.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Users.Infrastructure.Persistence;

public sealed class UsersInboxStore(UsersDbContext dbContext) : IUsersInboxStore
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
