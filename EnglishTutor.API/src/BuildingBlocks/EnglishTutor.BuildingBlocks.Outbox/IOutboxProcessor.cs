namespace EnglishTutor.BuildingBlocks.Outbox;

public interface IOutboxProcessor
{
    Task ProcessPendingMessagesAsync(CancellationToken ct = default);
}
