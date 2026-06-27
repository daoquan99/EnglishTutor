using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.BuildingBlocks.Application.DomainEvents;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken);
}
