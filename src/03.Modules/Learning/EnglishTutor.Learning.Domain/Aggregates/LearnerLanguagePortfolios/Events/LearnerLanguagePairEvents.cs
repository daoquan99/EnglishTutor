using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios.Events;

public sealed record LearnerLanguagePairAddedDomainEvent(
    Guid PortfolioId,
    Guid UserId,
    Guid LanguagePairId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}

public sealed record LearnerActiveLanguagePairChangedDomainEvent(
    Guid PortfolioId,
    Guid UserId,
    Guid LanguagePairId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}

public sealed record LearnerLanguagePairArchivedDomainEvent(
    Guid PortfolioId,
    Guid UserId,
    Guid LanguagePairId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
