using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Events;

public sealed record ModeDefinitionCreatedDomainEvent(
    Guid ModeDefinitionId,
    string Code,
    string Name,
    Guid? CreatedByUserId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
