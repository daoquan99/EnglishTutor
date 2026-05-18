using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Users.Domain.Events;

public sealed record UserLevelChangedDomainEvent(
    Guid UserId,
    string TargetLanguageCode,
    string PreviousLevel,
    string NewLevel,
    DateTime ChangedAtUtc) : DomainEvent;
