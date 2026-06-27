using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.BuildingBlocks.Infrastructure.DomainEvents;

/// <summary>
/// Transaction-bound, sequential, fail-fast implementation of ITransactionalDomainEventDispatcher.
/// Any exception from a domain event handler propagates immediately to roll back the active transaction.
/// </summary>
public sealed class TransactionalDomainEventDispatcher : ITransactionalDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TransactionalDomainEventDispatcher> _logger;

    public TransactionalDomainEventDispatcher(
        IServiceProvider serviceProvider,
        ILogger<TransactionalDomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> events,
        CancellationToken cancellationToken)
    {
        var batch = events?.ToList() ?? new List<IDomainEvent>();
        if (batch.Count == 0)
        {
            return;
        }

        foreach (var domainEvent in batch)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await DispatchOneAsync(
                domainEvent: domainEvent,
                ct: cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task DispatchOneAsync(IDomainEvent domainEvent, CancellationToken ct)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

        // Resolve all handlers for this event. Zero-handlers is allowed
        // (domain-only events; handled by module-level mapping tests for integration events).
        var handlers = _serviceProvider.GetServices(handlerType).ToArray();

        var dispatched = 0;
        foreach (var handler in handlers)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))
                    ?? throw new InvalidOperationException(
                        $"Handler {handler!.GetType().FullName} is missing HandleAsync.");

                var result = handleMethod.Invoke(
                    obj: handler,
                    parameters: new object?[] { domainEvent, ct });

                if (result is Task task)
                {
                    await task.ConfigureAwait(false);
                }
                dispatched++;
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                _logger.LogError(
                    ex.InnerException,
                    "Transactional domain event handler {Handler} failed for event {EventType} (EventId={EventId}). Aborting transaction.",
                    handler?.GetType().FullName ?? "<null>",
                    eventType.FullName,
                    domainEvent.EventId);

                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Transactional domain event handler {Handler} failed for event {EventType} (EventId={EventId}). Aborting transaction.",
                    handler?.GetType().FullName ?? "<null>",
                    eventType.FullName,
                    domainEvent.EventId);

                throw;
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
