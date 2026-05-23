using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Modules.Users.Application.Abstractions;

namespace EnglishTutor.Modules.Users.Infrastructure.Persistence;

public sealed class UsersInboxStore(
    UsersDbContext dbContext,
    IDateTimeProvider dateTimeProvider) : IUsersInboxStore
{
    public Task<bool> IsProcessedAsync(Guid eventId, string handlerName, CancellationToken cancellationToken) =>
        EfCoreInboxStore.IsProcessedAsync(dbContext.InboxMessages, eventId, handlerName, cancellationToken);

    public Task MarkProcessedAsync(Guid eventId, string eventType, string handlerName, CancellationToken cancellationToken)
    {
        EfCoreInboxStore.MarkProcessed(dbContext.InboxMessages, dateTimeProvider, eventId, eventType, handlerName);

        return Task.CompletedTask;
    }
}
