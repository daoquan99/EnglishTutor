namespace EnglishTutor.BuildingBlocks.EventBus;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
    string EventType { get; }
}
