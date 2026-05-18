# ADR-0004: Internal Messaging with Outbox/Inbox Pattern

## Status
Accepted

## Context
Integration events must be delivered reliably. Options:
1. **In-memory event dispatch** — fast but events lost on crash
2. **RabbitMQ/Kafka** — reliable but adds infrastructure complexity
3. **DB-backed Outbox/Inbox** — reliable, no extra infrastructure, fits modular monolith

## Decision
Use **Transactional Outbox** per producer module + **Inbox** per consumer module + **Dead-letter table**.

### Flow
1. API handler saves business data + `OutboxMessage` in **same DB transaction**
2. Worker polls `OutboxMessages` (Status=Pending), locks rows
3. Worker deserializes event, dispatches to handlers via `IEventBus`
4. Each handler checks `InboxMessage` for idempotency before processing
5. On success: mark Inbox + mark Outbox as Processed
6. On failure: increment RetryCount, calculate NextRetryAtUtc (exponential backoff)
7. After MaxRetryCount (5): move to `DeadLetterMessages`

### Retry Policy
`NextRetryAtUtc = UtcNow + 2^retryCount × 10 seconds`

## Consequences
**Benefits:**
- No event loss — events are persisted before acknowledgment
- No extra infrastructure (no RabbitMQ/Kafka to manage)
- Idempotent consumers via Inbox
- Dead-letter for manual investigation
- Worker can scale horizontally via row locking

**Trade-offs:**
- Polling-based (5s default) — not truly real-time
- Adds DB load from polling
- Must serialize/deserialize events as JSON
- Can migrate to RabbitMQ later by swapping `IEventBus` implementation
