using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Users.Contracts.IntegrationEvents;

public sealed record UserLanguageSettingsUpdatedIntegrationEvent(
    Guid UserId,
    string NativeLanguageCode,
    string UiLanguageCode,
    string ExplanationLanguageCode,
    string ActiveTargetLanguageCode,
    DateTime UpdatedAtUtc) : IntegrationEvent;
