using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Users.Domain.Events;

public sealed record UserTargetLanguageActivationChangedDomainEvent(
    Guid UserId,
    string TargetLanguageCode,
    string CurrentLevel,
    string TargetLevel,
    bool IsActive,
    DateTime ChangedAtUtc) : DomainEvent;
