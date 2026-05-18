using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Mistakes.Domain.Events;

public sealed record MistakeReviewedDomainEvent(
    Guid UserId,
    Guid MistakeId,
    string TargetLanguageCode,
    DateTime ReviewedAtUtc) : DomainEvent;
