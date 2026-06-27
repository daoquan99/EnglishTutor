using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Events;

public sealed record ModeDefinitionUpdatedDomainEvent(
    Guid ModeDefinitionId,
    string Code,
    string Name,
    string Description,
    Guid? UpdatedByUserId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
