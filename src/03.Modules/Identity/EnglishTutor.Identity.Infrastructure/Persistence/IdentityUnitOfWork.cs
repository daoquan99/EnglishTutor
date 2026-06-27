using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Identity.Infrastructure.Persistence;

internal sealed class IdentityUnitOfWork : IIdentityUnitOfWork
{
    private readonly IdentityDbContext _db;
    private readonly ITransactionalDomainEventDispatcher _dispatcher;

    public IdentityUnitOfWork(IdentityDbContext db, ITransactionalDomainEventDispatcher dispatcher)
    {
        _db = db;
        _dispatcher = dispatcher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        var aggregates = _db.ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToArray();

        var events = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToArray();

        if (events.Length == 0)
        {
            return await _db.SaveChangesAsync(cancellationToken);
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        await _dispatcher.DispatchAsync(
            events: events,
            cancellationToken: cancellationToken);

        var result = await _db.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        return result;
    }
}
