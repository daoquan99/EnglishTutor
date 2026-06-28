namespace EnglishTutor.BuildingBlocks.Infrastructure.Outbox;

public interface IOutboxStore
{
    string ModuleName { get; }

    Task<IReadOnlyList<OutboxMessage>> ClaimAsync(
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken);

    Task MarkPublishedAsync(
        Guid messageId,
        Guid lockId,
        CancellationToken cancellationToken);

    Task MarkFailedAsync(
        Guid messageId,
        Guid lockId,
        string errorCode,
        string errorMessage,
        TimeSpan retryDelay,
        int maxAttempts,
        CancellationToken cancellationToken);
}
