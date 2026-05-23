using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Outbox;

namespace EnglishTutor.Modules.Auth.Infrastructure.Persistence;

public interface IAuthDomainEventToOutboxMapper
{
    OutboxMessage? Map(DomainEvent domainEvent);
}
