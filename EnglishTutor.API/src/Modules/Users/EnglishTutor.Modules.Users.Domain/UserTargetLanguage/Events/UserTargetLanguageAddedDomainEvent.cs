using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Users.Domain.Events;

public sealed record UserTargetLanguageAddedDomainEvent(
    Guid UserId,
    string TargetLanguageCode,
    string CurrentLevel,
    string TargetLevel,
    bool IsActive) : DomainEvent;
