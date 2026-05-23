using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Modules.LearningContent.Application.Abstractions;

namespace EnglishTutor.Modules.LearningContent.Infrastructure.Persistence;

public sealed class LearningContentInboxStore(
    LearningContentDbContext dbContext,
    IDateTimeProvider dateTimeProvider) : ILearningContentInboxStore
{
    public Task<bool> IsProcessedAsync(Guid eventId, string handlerName, CancellationToken cancellationToken) =>
        EfCoreInboxStore.IsProcessedAsync(dbContext.InboxMessages, eventId, handlerName, cancellationToken);

    public Task MarkProcessedAsync(Guid eventId, string eventType, string handlerName, CancellationToken cancellationToken)
    {
        EfCoreInboxStore.MarkProcessed(dbContext.InboxMessages, dateTimeProvider, eventId, eventType, handlerName);

        return Task.CompletedTask;
    }
}
