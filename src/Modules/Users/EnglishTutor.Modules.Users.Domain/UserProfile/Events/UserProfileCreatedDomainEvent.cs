using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Users.Domain.Events;

public sealed record UserProfileCreatedDomainEvent(
    Guid UserId,
    string DisplayName) : DomainEvent;
