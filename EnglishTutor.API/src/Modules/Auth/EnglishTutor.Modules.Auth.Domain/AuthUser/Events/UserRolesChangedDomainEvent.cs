using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Auth.Domain.AuthUser.Events;

public sealed record UserRolesChangedDomainEvent(
    Guid UserId,
    IReadOnlyCollection<Guid> NewRoleIds) : DomainEvent;
