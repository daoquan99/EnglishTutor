using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Auth.Contracts.IntegrationEvents;

public sealed record RolePermissionsChangedIntegrationEvent(
    Guid RoleId,
    string RoleName,
    IReadOnlyCollection<Guid> AffectedUserIds,
    DateTime ChangedAtUtc) : IntegrationEvent;
