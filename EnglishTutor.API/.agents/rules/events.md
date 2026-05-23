# Events & messaging rules

## Two event types

- **Domain Event** — internal to a module. Raised by an aggregate (`AddDomainEvent`). Handled by the same module's MediatR pipeline. Never crosses module boundaries.
- **Integration Event** — public cross-module contract. Lives in `{Module}.Contracts/IntegrationEvents/`. Past tense (`VocabularyMasteredIntegrationEvent`, `SpeakingSessionCompletedIntegrationEvent`).

## Producer flow (in API host)

Inside the command handler, in a single DB transaction:

1. Mutate aggregate(s) and raise Domain Event(s).
2. Map Domain Event(s) to Integration Event(s).
3. Serialize the Integration Event into an `OutboxMessage` row in the **same module's** schema (each producer module has its own outbox table; the table mirrors `messaging.OutboxMessages` shape).
4. `SaveChangesAsync` — business data + outbox row commit atomically.
5. Return the HTTP response.

Never publish integration events directly from in-memory request threads. Never write to an outbox table belonging to a different module.

## Worker dispatch

`OutboxProcessingJob` (Quartz) runs in `EnglishTutor.Worker`:

1. Lease unprocessed `OutboxMessage` rows: `SELECT ... FOR UPDATE SKIP LOCKED` (uses lock columns `LockedBy`, `LockedUntilUtc`, `RetryCount`, `NextRetryAtUtc`).
2. Publish each event via the in-process EventBus.
3. On success: mark `ProcessedOnUtc`.
4. On failure: increment `RetryCount`, set `NextRetryAtUtc` with exponential backoff. Beyond `MaxRetries`, move to `messaging.DeadLetterMessages` with the error context.

Worker scales horizontally — row-level leasing prevents double dispatch.

## Consumer flow

Consumer is an `INotificationHandler<TIntegrationEvent>` in `{Module}.Application/EventHandlers/`:

1. Check `messaging.InboxMessages` for `(EventId, ConsumerName)` — if already processed, return.
2. Execute the business mutation (must be idempotent — at-least-once delivery).
3. Insert/upsert the inbox row in the same transaction as the business mutation.

Idempotency belongs in the handler, not the EventBus.

## Conventions

- Event name: past tense, suffix `IntegrationEvent`. Live in `{Module}.Contracts/IntegrationEvents/`.
- Payload fields: only primitives/value-object primitives + IDs. No EF entities. No `IQueryable`. All timestamps UTC with `Utc` suffix.
- Include the originating aggregate ID and any IDs the consumer needs to look up further detail via a Contract Reader.
- Bump a version field if you change payload shape after release; do not silently re-shape an event.

## Documentation

Every integration event must be documented in `docs/events/integration-events.md` with: producer, consumers, payload fields, when published, side effects, idempotency notes, related workflow.

`docs/events/outbox-inbox.md` is the reference for the messaging tables and Worker job behavior.

## Forbidden

- Publishing an integration event without an `OutboxMessage`.
- Writing the outbox row in a different transaction from the business data.
- Crossing module boundaries with a Domain Event.
- Adding `EventHandlerExecutions` table.
- Adding RabbitMQ/Kafka.
- Skipping the Inbox idempotency check in a consumer.
- Mutating an event's payload contract without a version bump and doc update.
