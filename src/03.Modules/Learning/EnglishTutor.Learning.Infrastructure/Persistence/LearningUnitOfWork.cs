using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.Learning.Application.Abstractions.Persistence;

namespace EnglishTutor.Learning.Infrastructure.Persistence;

internal sealed class LearningUnitOfWork : ILearningUnitOfWork
{
    private readonly LearningDbContext _dbContext;
    private readonly ITransactionalDomainEventDispatcher _dispatcher;

    public LearningUnitOfWork(
        LearningDbContext dbContext,
        ITransactionalDomainEventDispatcher dispatcher)
    {
        _dbContext = dbContext;
        _dispatcher = dispatcher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = _dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToArray();

        if (aggregates.Length == 0)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var domainEvents = aggregates
            .SelectMany(aggregate => aggregate.DomainEvents)
            .ToArray();

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await _dispatcher.DispatchAsync(
            events: domainEvents,
            cancellationToken: cancellationToken);

        var result = await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        return result;
    }
}
