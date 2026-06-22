using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.BuildingBlocks.Application.DomainEvents;

// Handles a single type of domain event. One handler per (event type, concern) pair.
// Multiple handlers for the same event are allowed (e.g. one records to audit,
// another projects to a read model).
//
// Implementations are resolved from DI by the in-process dispatcher in
// BuildingBlocks.Infrastructure. Handlers MUST be safe to call from inside
// an already-saved transaction (after the producing aggregate has been
// persisted) and MUST NOT mutate the producing aggregate.
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}
