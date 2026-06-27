namespace EnglishTutor.BuildingBlocks.Application.DomainEvents;

public interface IEventEnvelopeContextAccessor
{
    EventEnvelopeContext Current { get; }
}
