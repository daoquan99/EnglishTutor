using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnglishTutor.Worker.Outbox;

public sealed class EfCoreOutboxProcessor(
    IEnumerable<IModuleOutboxStore> outboxStores,
    MessagingDbContext messagingDbContext,
    OutboxMessageDispatcher dispatcher,
    IDateTimeProvider dateTimeProvider,
    IOptions<OutboxOptions> options,
    ILogger<EfCoreOutboxProcessor> logger) : IOutboxProcessor
{
    private readonly OutboxOptions _options = options.Value;
    private readonly string _workerId = $"{Environment.MachineName}-{Guid.NewGuid():N}";

    public async Task ProcessPendingMessagesAsync(CancellationToken ct = default)
    {
        foreach (var store in outboxStores)
        {
            try
            {
                await ProcessStoreAsync(store.ModuleName, store.OutboxMessages, store.DbContext, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to process outbox store {ModuleName}. Verify that the module migrations are applied and the {Schema}.OutboxMessages table exists.",
                    store.ModuleName,
                    store.ModuleName);
            }
        }
    }

    private async Task ProcessStoreAsync(string moduleName, DbSet<OutboxMessage> outboxMessages, DbContext dbContext, CancellationToken ct)
    {
        var now = dateTimeProvider.UtcNow;
        var messages = await LeaseMessagesAsync(moduleName, outboxMessages, dbContext, now, ct);

        foreach (var message in messages)
        {
            var deadLetter = await dispatcher.DispatchAsync(message, ct);

            // Persist the dead-letter row FIRST so that if the subsequent outbox-state SaveChanges
            // crashes, the row remains Processing/expired and gets re-leased; the re-dispatch will
            // produce another DeadLetterMessage but the EventId unique index dedupes it.
            if (deadLetter is not null)
            {
                deadLetter.SourceModule = moduleName;
                messagingDbContext.DeadLetterMessages.Add(deadLetter);
                try
                {
                    await messagingDbContext.SaveChangesAsync(ct);
                }
                catch (DbUpdateException ex) when (IsUniqueViolation(ex))
                {
                    // A previous attempt for the same EventId already persisted the dead letter.
                    // Detach the duplicate so the next SaveChanges doesn't replay it.
                    messagingDbContext.Entry(deadLetter).State = EntityState.Detached;
                    logger.LogWarning(
                        "Dead letter for event {EventId} already persisted; deduping duplicate.",
                        deadLetter.EventId);
                }
            }

            await dbContext.SaveChangesAsync(ct);
        }
    }

    private async Task<IReadOnlyList<OutboxMessage>> LeaseMessagesAsync(
        string schemaName,
        DbSet<OutboxMessage> outboxMessages,
        DbContext dbContext,
        DateTime now,
        CancellationToken cancellationToken)
    {
        ValidateSchemaName(schemaName);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // PostgreSQL identifiers cannot be parameterized. Keep schemaName validated by ValidateSchemaName before interpolation.
        #pragma warning disable EF1002
        var messages = await outboxMessages
            .FromSqlRaw(
                $$"""
                SELECT *
                FROM "{{schemaName}}"."OutboxMessages"
                WHERE ("Status" = {0} OR ("Status" = {3} AND "LockedUntilUtc" <= {1}))
                  AND ("NextRetryAtUtc" IS NULL OR "NextRetryAtUtc" <= {1})
                  AND ("LockedUntilUtc" IS NULL OR "LockedUntilUtc" <= {1})
                ORDER BY "CreatedAtUtc"
                LIMIT {2}
                FOR UPDATE SKIP LOCKED
                """,
                OutboxMessageStatus.Pending.ToString(),
                now,
                _options.BatchSize,
                OutboxMessageStatus.Processing.ToString())
            .ToListAsync(cancellationToken);
        #pragma warning restore EF1002

        var lockedUntil = now.AddSeconds(_options.LockDurationSeconds);
        foreach (var message in messages)
        {
            message.Status = OutboxMessageStatus.Processing;
            message.LockedBy = _workerId;
            message.LockedUntilUtc = lockedUntil;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return messages;
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is Npgsql.PostgresException pg && pg.SqlState == "23505";

    private static void ValidateSchemaName(string schemaName)
    {
        if (schemaName.Any(character => !char.IsLower(character) && !char.IsDigit(character)))
        {
            throw new InvalidOperationException($"Invalid outbox schema name '{schemaName}'.");
        }
    }
}
