namespace EnglishTutor.Modules.Mistakes.Application.Abstractions;

public interface IMistakesInboxStore
{
    Task<bool> IsProcessedAsync(Guid eventId, string handlerName, CancellationToken cancellationToken);

    Task MarkProcessedAsync(Guid eventId, string eventType, string handlerName, CancellationToken cancellationToken);
}
