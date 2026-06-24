using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Learning.Domain.Aggregates.Topics.Events;

public sealed record TopicCreatedDomainEvent(
    Guid TopicId,
    string Name,
    string Slug,
    Guid? CreatedByUserId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
