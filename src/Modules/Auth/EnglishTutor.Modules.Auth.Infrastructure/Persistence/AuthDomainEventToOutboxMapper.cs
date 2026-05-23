using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.Auth.Contracts.IntegrationEvents;
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
            _ => null
        };

    private OutboxMessage CreateOutboxMessage(UserRegisteredIntegrationEvent integrationEvent) =>
        OutboxMessageFactory.Create(integrationEvent, "auth", serializer.Serialize(integrationEvent));
}
