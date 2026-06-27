using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.BuildingBlocks.Application.DomainEvents;

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}
