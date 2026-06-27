using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.BuildingBlocks.Application.DomainEvents;

public interface ITransactionalDomainEventDispatcher
{
    Task DispatchAsync(
        IEnumerable<IDomainEvent> events,
        CancellationToken cancellationToken);
}
