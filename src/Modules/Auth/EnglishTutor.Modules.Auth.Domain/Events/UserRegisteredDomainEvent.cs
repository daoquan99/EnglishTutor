using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Auth.Domain.Events;

public sealed record UserRegisteredDomainEvent(
    Guid UserId,
    string Email,
    string DisplayName) : DomainEvent;
