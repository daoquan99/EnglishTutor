using EnglishTutor.BuildingBlocks.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Persistence;

public static class DomainEventDispatcher
{
    public static async Task DispatchDomainEventsAsync(this IMediator mediator, DbContext context, CancellationToken ct = default)
    {
        var domainEventHolders = context.ChangeTracker
            .Entries<IDomainEventHolder>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = domainEventHolders
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var holder in domainEventHolders)
            holder.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
            await mediator.Publish(domainEvent, ct);
    }
}
