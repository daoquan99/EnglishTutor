namespace EnglishTutor.Modules.LearningContent.Application.Abstractions;

public interface ILearningContentInboxStore
{
    Task<bool> IsProcessedAsync(Guid eventId, string handlerName, CancellationToken cancellationToken);

    Task MarkProcessedAsync(Guid eventId, string eventType, string handlerName, CancellationToken cancellationToken);
}
