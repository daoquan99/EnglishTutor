using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Auth.Domain.AuthRole.Events;

public sealed record RolePermissionsChangedDomainEvent(
    Guid RoleId,
    string RoleName,
    IReadOnlyCollection<Guid> AffectedUserIds) : DomainEvent;
