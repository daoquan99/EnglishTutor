using System.Reflection;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using EnglishTutor.Identity.Application.Abstractions.Persistence;

namespace EnglishTutor.Identity.Infrastructure.Persistence;

/// <summary>
/// Infrastructure implementation of <see cref="IIdentityUnitOfWork"/>.
/// Wraps the <c>IdentityDbContext.SaveChangesAsync</c> call AND
/// dispatches any domain events collected on the tracked aggregates AFTER
/// the SaveChanges commits. Registered as scoped so it shares the same
/// <c>IdentityDbContext</c> instance as the request handlers.
///
/// <para><b>Dispatch timing:</b> AFTER SaveChanges. The Identity row is
/// committed before any handler runs. Audit handlers (and any future
/// handler) are BEST-EFFORT in-process. NOT outbox-level reliability. A
/// failed Audit recording does NOT roll back the Identity change. See
/// Task 22A design for the explicit "not outbox-level reliability"
/// caveat.</para>
/// </summary>
internal sealed class IdentityUnitOfWork : IIdentityUnitOfWork
{
    private static readonly MethodInfo ClearDomainEventsMethod =
        typeof(AggregateRoot).GetMethod(nameof(AggregateRoot.ClearDomainEvents))
        ?? throw new InvalidOperationException("AggregateRoot.ClearDomainEvents not found.");

    private readonly IdentityDbContext _db;
    private readonly IDomainEventDispatcher _dispatcher;

    public IdentityUnitOfWork(IdentityDbContext db, IDomainEventDispatcher dispatcher)
    {
        _db = db;
        _dispatcher = dispatcher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        // Collect events from all tracked aggregates BEFORE SaveChanges.
        var aggregates = _db.ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var events = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        var result = await _db.SaveChangesAsync(cancellationToken);

        // Dispatch AFTER SaveChanges. Best-effort, fail-soft. See type doc.
        if (events.Count > 0)
        {
            try
            {
                await _dispatcher.DispatchAsync(events, cancellationToken);
            }
            finally
            {
                // Always clear events to prevent re-dispatch on the next SaveChanges.
                foreach (var aggregate in aggregates)
                {
                    ClearDomainEventsMethod.Invoke(aggregate, null);
                }
            }
        }

        return result;
    }
}
