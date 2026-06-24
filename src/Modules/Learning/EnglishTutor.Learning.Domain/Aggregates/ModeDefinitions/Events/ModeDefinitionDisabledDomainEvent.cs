using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Events;

public sealed record ModeDefinitionDisabledDomainEvent(
    Guid ModeDefinitionId,
    Guid? DisabledByUserId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
