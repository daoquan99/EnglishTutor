using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Users.Contracts.IntegrationEvents;

public sealed record UserLevelChangedIntegrationEvent(
    Guid UserId,
    string TargetLanguageCode,
    string PreviousLevel,
    string NewLevel,
    DateTime ChangedAtUtc) : IntegrationEvent;
