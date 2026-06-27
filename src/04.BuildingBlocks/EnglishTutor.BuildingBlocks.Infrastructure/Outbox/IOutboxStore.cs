using EnglishTutor.BuildingBlocks.Infrastructure.Outbox;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Abstraction for storing and retrieving outbox messages.
/// Implementation lives in each module's Infrastructure project (e.g., Practice, Learning)
/// to keep the table schema and DbContext module-owned.
/// </summary>
public interface IOutboxStore
{
    /// <summary>
    /// Adds a new outbox message to be persisted in the same database transaction
    /// as the business state change.
    /// </summary>
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a batch of unprocessed outbox messages, ordered by OccurredAtUtc.
    /// </summary>
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(
        int batchSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an outbox message as successfully published and processed.
    /// </summary>
    Task MarkProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a failure for an outbox message and increments its retry count.
    /// Failed messages remain eligible for retry until a max retry threshold is reached.
    /// </summary>
    Task MarkFailedAsync(
        Guid messageId,
        string error,
        CancellationToken cancellationToken = default);
}
