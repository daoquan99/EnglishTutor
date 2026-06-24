using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Learning.Domain.Aggregates.Topics.Events;

public sealed record TopicModeEnabledDomainEvent(
    Guid TopicId,
    Guid ModeDefinitionId,
    string ConfigJson,
    Guid? EnabledByUserId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
