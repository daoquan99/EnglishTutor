---
name: integration-events-agent
description: Wires Outbox/Inbox plumbing — producer side (DomainEvent → IntegrationEvent → OutboxMessage in one tx) and consumer side (Inbox idempotency + business mutation). Also wires Worker jobs and projections.
---

# Integration Events Agent

## When to invoke

- Adding a new integration event.
- Wiring a new event consumer.
- Adding a Worker job (outbox processing, scheduled task, projection rebuild).
- Implementing a Read Model / Projection.
- Debugging at-least-once delivery, retry, or dead-letter behavior.

## Producer side

### 1. Define the integration event

`{Module}.Contracts/IntegrationEvents/{Verb}{Aggregate}IntegrationEvent.cs`:

```csharp
public sealed record VocabularyMasteredIntegrationEvent(
    Guid EventId,
    Guid UserId,
    Guid VocabularyItemId,
    string TargetLanguageCode,
    DateTime OccurredOnUtc) : IntegrationEvent(EventId, OccurredOnUtc);
```

- Past tense.
- Primitives + IDs + UTC timestamps (with `Utc` suffix). No EF entities.

### 2. Raise the domain event in the aggregate

```csharp
public void MarkMastered(DateTime nowUtc)
{
    Status = MasteryStatus.Mastered;
    MasteredAtUtc = nowUtc;
    AddDomainEvent(new VocabularyMastered(UserId, ItemId, LanguageCode, nowUtc));
}
```

### 3. Map domain → integration event in Application

`{Module}.Application/EventHandlers/{DomainEventName}Handler.cs` (an `INotificationHandler<TDomainEvent>` inside the module):

```csharp
public async Task Handle(VocabularyMastered domainEvent, CancellationToken ct)
{
    var integrationEvent = new VocabularyMasteredIntegrationEvent(
        Guid.NewGuid(),
        domainEvent.UserId,
        domainEvent.ItemId,
        domainEvent.LanguageCode,
        domainEvent.OccurredOnUtc);

    await _outbox.AddAsync(integrationEvent, ct);     // module-local outbox
}
```

The outbox add and the business `SaveChangesAsync` happen in the same DbContext, same transaction. EF SaveChanges commits both atomically.

## Consumer side

### 1. Handler in the consuming module

`{ConsumerModule}.Application/EventHandlers/{EventName}Handler.cs`:

```csharp
public sealed class OnVocabularyMastered(IInbox inbox, IProgressService progress)
    : INotificationHandler<VocabularyMasteredIntegrationEvent>
{
    public async Task Handle(VocabularyMasteredIntegrationEvent evt, CancellationToken ct)
    {
        if (await inbox.WasProcessedAsync(evt.EventId, nameof(OnVocabularyMastered), ct))
            return;

        await progress.AwardExpAsync(evt.UserId, evt.TargetLanguageCode, EXPDelta.VocabMastered, ct);

        await inbox.MarkProcessedAsync(evt.EventId, nameof(OnVocabularyMastered), ct);
    }
}
```

Both the business mutation and the inbox row are in the same DbContext transaction.

### 2. Idempotency

- Key: `(EventId, ConsumerName)`.
- The handler must be safe to invoke twice — at-least-once delivery is the guarantee.
- Side effects to third parties (email, push) must use their own idempotency keys or be deduplicated on receipt.

## Worker jobs

Live in `src/Bootstrapper/EnglishTutor.Worker/Jobs/`. Existing jobs:

- `OutboxProcessingJob` — leases rows, dispatches events, retries with backoff, dead-letters past `MaxRetries`.
- `MissedSessionDetectionJob` — finds overdue planned sessions, raises `PlannedStudySessionMissedIntegrationEvent`.
- `ReportGenerationJobs` — periodic projection-fed reports.
- `StudyReminderSchedulingJob` — pulls due reminders into the notification queue.

Adding a new job:

- Inherit from the project's `IJob` (Quartz).
- Schedule in `EnglishTutor.Worker/DependencyInjection.cs`.
- Idempotent body; use row leasing for shared state.
- Use `IDateTimeProvider.UtcNow`, never `DateTime.Now`.

## Outbox row schema (per producer module)

Standard columns:

```
Id (Guid), EventId (Guid), EventType (text), PayloadJson (text),
OccurredOnUtc, ProcessedOnUtc (nullable), RetryCount (int),
LockedBy (text, nullable), LockedUntilUtc (nullable), NextRetryAtUtc (nullable),
Error (text, nullable)
```

Lease query in Worker:

```sql
SELECT * FROM {module}.OutboxMessages
WHERE ProcessedOnUtc IS NULL
  AND (LockedUntilUtc IS NULL OR LockedUntilUtc < now())
  AND (NextRetryAtUtc IS NULL OR NextRetryAtUtc <= now())
ORDER BY OccurredOnUtc
FOR UPDATE SKIP LOCKED
LIMIT @batch
```

## Dead letter

After `MaxRetries`, move the row (or copy + mark) into `messaging.DeadLetterMessages` with the last error context. Preserve `EventId` so manual reprocessing remains idempotent on the consumer side.

## Forbidden

- Publishing an integration event directly from in-memory request thread.
- Writing the outbox row in a separate transaction from the business data.
- Crossing module boundaries with a domain event.
- Skipping the inbox check in a consumer.
- Adding RabbitMQ/Kafka.
- Adding `EventHandlerExecutions` table.

## Done when

- New event has: contract record + producer handler + outbox add + at least one consumer.
- Consumer uses inbox idempotency.
- Integration test in `tests/EnglishTutor.IntegrationTests/` exercises outbox→dispatch→inbox happy path.
- Event documented in `docs/events/integration-events.md`.
- If introducing a Worker job: scheduled in DI and idempotent.
