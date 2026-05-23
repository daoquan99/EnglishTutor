using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Users.Contracts.IntegrationEvents;

public sealed record UserTargetLanguageChangedIntegrationEvent(
    Guid UserId,
    string TargetLanguageCode,
    string CurrentLevel,
    string TargetLevel,
    bool IsActive,
    DateTime ChangedAtUtc) : IntegrationEvent;
