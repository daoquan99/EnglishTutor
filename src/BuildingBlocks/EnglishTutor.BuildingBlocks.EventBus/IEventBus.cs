namespace EnglishTutor.BuildingBlocks.EventBus;

public interface IEventBus
{
    Task PublishAsync(IIntegrationEvent @event, CancellationToken ct = default);
}
