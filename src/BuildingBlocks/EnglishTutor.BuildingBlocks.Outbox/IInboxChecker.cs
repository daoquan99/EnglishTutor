namespace EnglishTutor.BuildingBlocks.Outbox;

public interface IInboxChecker
{
    Task<bool> IsProcessedAsync(Guid eventId, string handlerName, CancellationToken ct = default);
    Task MarkAsProcessedAsync(Guid eventId, string eventType, string handlerName, CancellationToken ct = default);
}
