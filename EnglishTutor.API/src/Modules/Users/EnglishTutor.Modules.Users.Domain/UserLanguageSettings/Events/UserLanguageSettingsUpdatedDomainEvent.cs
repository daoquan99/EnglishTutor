using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Users.Domain.Events;

public sealed record UserLanguageSettingsUpdatedDomainEvent(
    Guid UserId,
    string NativeLanguageCode,
    string UiLanguageCode,
    string ExplanationLanguageCode,
    string ActiveTargetLanguageCode) : DomainEvent;
