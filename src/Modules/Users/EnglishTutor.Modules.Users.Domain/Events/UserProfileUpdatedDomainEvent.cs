using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Users.Domain.Events;

public sealed record UserProfileUpdatedDomainEvent(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    string? Bio) : DomainEvent;
