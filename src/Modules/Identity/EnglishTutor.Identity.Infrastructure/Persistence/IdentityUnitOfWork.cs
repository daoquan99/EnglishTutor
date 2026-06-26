using System.Reflection;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Identity.Infrastructure.Persistence;

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
        var aggregates = _db.ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var events = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        if (events.Count == 0)
        {
            return await _db.SaveChangesAsync(cancellationToken);
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        await _dispatcher.DispatchAsync(events, cancellationToken);

        var result = await _db.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        foreach (var aggregate in aggregates)
        {
            ClearDomainEventsMethod.Invoke(aggregate, null);
        }

        return result;
    }
}
