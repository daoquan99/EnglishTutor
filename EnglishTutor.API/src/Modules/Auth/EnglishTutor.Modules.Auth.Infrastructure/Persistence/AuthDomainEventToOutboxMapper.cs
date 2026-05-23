using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.Auth.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Auth.Domain.AuthRole.Events;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Events;

namespace EnglishTutor.Modules.Auth.Infrastructure.Persistence;

public sealed class AuthDomainEventToOutboxMapper(JsonSerializerService serializer) : IAuthDomainEventToOutboxMapper
{
    public OutboxMessage? Map(DomainEvent domainEvent) =>
        domainEvent switch
        {
            UserRegisteredDomainEvent userRegistered => CreateOutboxMessage(
                new UserRegisteredIntegrationEvent(
                    userRegistered.UserId,
                    userRegistered.Email,
                    userRegistered.DisplayName,
                    userRegistered.OccurredOnUtc)
                {
                    EventId = userRegistered.EventId,
                    OccurredOnUtc = userRegistered.OccurredOnUtc
                }),
            RolePermissionsChangedDomainEvent rolePermissionsChanged => CreateOutboxMessage(
                new RolePermissionsChangedIntegrationEvent(
                    rolePermissionsChanged.RoleId,
                    rolePermissionsChanged.RoleName,
                    rolePermissionsChanged.AffectedUserIds,
                    rolePermissionsChanged.OccurredOnUtc)
                {
                    EventId = rolePermissionsChanged.EventId,
                    OccurredOnUtc = rolePermissionsChanged.OccurredOnUtc
                }),
            UserRolesChangedDomainEvent userRolesChanged => CreateOutboxMessage(
                new UserRolesChangedIntegrationEvent(
                    userRolesChanged.UserId,
                    userRolesChanged.NewRoleIds,
                    userRolesChanged.OccurredOnUtc)
                {
                    EventId = userRolesChanged.EventId,
                    OccurredOnUtc = userRolesChanged.OccurredOnUtc
                }),
            _ => null
        };

    private OutboxMessage CreateOutboxMessage<TEvent>(TEvent integrationEvent)
        where TEvent : IntegrationEvent =>
        OutboxMessageFactory.Create(integrationEvent, "auth", serializer.Serialize(integrationEvent));
}
