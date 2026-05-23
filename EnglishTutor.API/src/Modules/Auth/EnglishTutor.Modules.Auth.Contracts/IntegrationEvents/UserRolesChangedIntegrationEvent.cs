using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Auth.Contracts.IntegrationEvents;

public sealed record UserRolesChangedIntegrationEvent(
    Guid UserId,
    IReadOnlyCollection<Guid> NewRoleIds,
    DateTime ChangedAtUtc) : IntegrationEvent;
