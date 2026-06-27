namespace EnglishTutor.BuildingBlocks.Domain.DomainEvents;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAtUtc { get; }
}
