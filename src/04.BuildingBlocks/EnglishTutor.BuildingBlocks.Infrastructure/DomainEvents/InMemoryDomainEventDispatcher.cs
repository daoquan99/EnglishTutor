using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.BuildingBlocks.Infrastructure.DomainEvents;

// In-process, sequential, fail-soft implementation of IDomainEventDispatcher.
//
// Semantics:
//   - Each event is dispatched to ALL IDomainEventHandler<TEvent>
//     implementations registered in DI.
//   - Dispatch is sequential (not parallel). One event finishes dispatch
//     (or its handlers fail) before the next event is dispatched.
//   - A handler exception is CAUGHT, LOGGED, and dispatch CONTINUES to the
//     next event. The Identity SaveChanges has already returned at this
//     point; the Identity row is committed. The audit record (if any) is
//     best-effort. See Task 22A design for the explicit "not outbox-level
//     reliability" caveat.
//   - This is NOT transactional with the producing aggregate. A failed
//     audit recording does NOT roll back the Identity change.
internal sealed class InMemoryDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InMemoryDomainEventDispatcher> _logger;

    public InMemoryDomainEventDispatcher(
        IServiceProvider serviceProvider,
        ILogger<InMemoryDomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> events,
        CancellationToken cancellationToken)
    {
        // Materialise once so the caller can safely reuse the source.
        var batch = events?.ToList() ?? new List<IDomainEvent>();
        if (batch.Count == 0)
        {
            return;
        }

        foreach (var domainEvent in batch)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "Domain event dispatch cancelled. Remaining events: {Count}.",
                    batch.Count - batch.IndexOf(domainEvent));
                return;
            }

            await DispatchOneAsync(domainEvent, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task DispatchOneAsync(IDomainEvent domainEvent, CancellationToken ct)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

        // Resolve all handlers for this event. Empty enumeration is allowed
        // (no handler registered yet; the event is silently dropped, which is
        // the same behaviour as before this dispatcher existed).
        var handlers = _serviceProvider.GetServices(handlerType);

        var dispatched = 0;
        foreach (var handler in handlers)
        {
            try
            {
                var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))
                    ?? throw new InvalidOperationException(
                        $"Handler {handler!.GetType().FullName} is missing HandleAsync.");
                var result = handleMethod.Invoke(handler, new object?[] { domainEvent, ct });
                if (result is Task task)
                {
                    await task.ConfigureAwait(false);
                }
                dispatched++;
            }
            catch (Exception ex)
            {
                // Unwrap reflection TargetInvocationException so the log
                // shows the real handler exception, not the wrapper.
                var real = ex is System.Reflection.TargetInvocationException tie && tie.InnerException is not null
                    ? tie.InnerException
                    : ex;
                _logger.LogError(
                    real,
                    "Domain event handler {Handler} failed for event {EventType} (EventId={EventId}). " +
                    "Dispatch continues to the next event. " +
                    "NOTE: producing aggregate was already committed; this is best-effort in-process dispatch, " +
                    "not outbox-level reliability.",
                    handler?.GetType().FullName ?? "<null>",
                    eventType.FullName,
                    domainEvent.EventId);
            }
        }

        if (dispatched == 0)
        {
            _logger.LogDebug(
                "No IDomainEventHandler registered for event {EventType} (EventId={EventId}).",
                eventType.FullName,
                domainEvent.EventId);
        }
    }
}
