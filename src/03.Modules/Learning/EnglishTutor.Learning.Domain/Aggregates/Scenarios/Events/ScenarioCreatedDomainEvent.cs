using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Learning.Domain.Aggregates.Scenarios.Events;

public sealed record ScenarioCreatedDomainEvent(
    Guid ScenarioId,
    Guid TopicId,
    Guid ModeDefinitionId,
    string Name,
    string DifficultyLevel,
    string PromptTemplate,
    Guid? CreatedByUserId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
