using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Learning.Domain.Aggregates.Topics.Events;

public sealed record TopicDisabledDomainEvent(
    Guid TopicId,
    Guid? DisabledByUserId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
