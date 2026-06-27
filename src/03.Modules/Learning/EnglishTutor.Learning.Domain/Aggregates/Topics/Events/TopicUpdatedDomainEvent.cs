using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Learning.Domain.Aggregates.Topics.Events;

public sealed record TopicUpdatedDomainEvent(
    Guid TopicId,
    string Name,
    string Slug,
    string Description,
    Guid? UpdatedByUserId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
