using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.BuildingBlocks.Application.DomainEvents;

// Dispatches a batch of domain events to their registered IDomainEventHandler
// implementations. Sequential, fail-soft (a handler exception is logged and
// dispatch continues to the next event), in-process only. NOT outbox-level
// reliability: see individual implementations for the precise semantics.
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken);
}
