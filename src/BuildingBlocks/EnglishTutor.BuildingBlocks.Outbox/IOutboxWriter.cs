namespace EnglishTutor.BuildingBlocks.Outbox;

public interface IOutboxWriter
{
    void Add(OutboxMessage message);
}
