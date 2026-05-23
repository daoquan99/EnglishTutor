using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Mistakes.Domain.Events;

public sealed record MistakeCreatedDomainEvent(
    Guid UserId,
    Guid MistakeId,
    string TargetLanguageCode,
    string Type,
    string SourceType,
    Guid SourceId,
    DateTime CreatedAtUtc) : DomainEvent;
