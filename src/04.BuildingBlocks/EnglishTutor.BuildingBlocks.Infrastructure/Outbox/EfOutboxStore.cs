using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Outbox;

public sealed class EfOutboxStore<TDbContext> : IOutboxStore
    where TDbContext : DbContext, IOutboxDbContext
{
    private readonly TDbContext _dbContext;

    public EfOutboxStore(TDbContext dbContext, string moduleName)
    {
        _dbContext = dbContext;
        ModuleName = moduleName;
    }

    public string ModuleName { get; }

    public async Task<IReadOnlyList<OutboxMessage>> ClaimAsync(
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var lockId = Guid.NewGuid();
        var candidateIds = await _dbContext.OutboxMessages
            .Where(message =>
                ((message.Status == OutboxMessageStatus.Pending ||
                  message.Status == OutboxMessageStatus.Failed) &&
                 message.NextAttemptAtUtc <= now) ||
                (message.Status == OutboxMessageStatus.Publishing &&
                 message.LockedUntilUtc < now))
            .OrderBy(message => message.OccurredAtUtc)
            .Select(message => message.Id)
            .Take(batchSize)
            .ToArrayAsync(cancellationToken);

        if (candidateIds.Length == 0)
        {
            return [];
        }

        await _dbContext.OutboxMessages
            .Where(message => candidateIds.Contains(message.Id) &&
                (((message.Status == OutboxMessageStatus.Pending ||
                   message.Status == OutboxMessageStatus.Failed) &&
                  message.NextAttemptAtUtc <= now) ||
                 (message.Status == OutboxMessageStatus.Publishing &&
                  message.LockedUntilUtc < now)))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(message => message.Status, OutboxMessageStatus.Publishing)
                    .SetProperty(message => message.LockId, lockId)
                    .SetProperty(message => message.LockedUntilUtc, now.Add(leaseDuration)),
                cancellationToken);

        return await _dbContext.OutboxMessages
            .Where(message => message.LockId == lockId)
            .OrderBy(message => message.OccurredAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task MarkPublishedAsync(
        Guid messageId,
        Guid lockId,
        CancellationToken cancellationToken)
    {
        var message = await _dbContext.OutboxMessages
            .SingleAsync(candidate => candidate.Id == messageId, cancellationToken);
        message.MarkPublished(lockId, DateTimeOffset.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(
        Guid messageId,
        Guid lockId,
        string errorCode,
        string errorMessage,
        TimeSpan retryDelay,
        int maxAttempts,
        CancellationToken cancellationToken)
    {
        var message = await _dbContext.OutboxMessages
            .SingleAsync(candidate => candidate.Id == messageId, cancellationToken);
        message.MarkFailed(
            lockId: lockId,
            errorCode: errorCode,
            errorMessage: errorMessage,
            nextAttemptAtUtc: DateTimeOffset.UtcNow.Add(retryDelay),
            maxAttempts: maxAttempts);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
