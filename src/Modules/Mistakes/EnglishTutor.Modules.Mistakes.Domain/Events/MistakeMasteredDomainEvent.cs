using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Mistakes.Domain.Events;

public sealed record MistakeMasteredDomainEvent(
    Guid UserId,
    Guid MistakeId,
    string TargetLanguageCode,
    DateTime MasteredAtUtc) : DomainEvent;
