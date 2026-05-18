using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Vocabulary.Domain.Events;

public sealed record VocabularyMasteredDomainEvent(
    Guid UserId,
    Guid VocabularyItemId,
    string TargetLanguageCode,
    DateTime MasteredAtUtc) : DomainEvent;
