# Outbox / Inbox Messaging Design

## Architecture
```
In-process Event Dispatcher
+ Transactional Outbox per producer module
+ Inbox per consumer module
+ Retry with exponential backoff
+ Dead-letter table (messaging.DeadLetterMessages)
+ Worker host processes outbox
```

## Outbox (per producer module schema)
| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | PK |
| EventId | Guid | Unique event identifier |
| EventType | string | Full type name for deserialization |
| Payload | string | JSON serialized event |
| Status | enum | Pending / Processing / Processed / Failed |
| RetryCount | int | Current retry attempt |
| MaxRetryCount | int | Default 5 |
| NextRetryAtUtc | DateTime? | Exponential backoff |
| LockedBy | string? | Worker instance ID |
| LockedUntilUtc | DateTime? | Lock expiry for horizontal scaling |
| CreatedAtUtc | DateTime | When event was created |
| ProcessedAtUtc | DateTime? | When successfully processed |
| LastError | string? | Last error message |

## Inbox (per consumer module schema)
| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | PK |
| EventId | Guid | From the event |
| EventType | string | Event type |
| HandlerName | string | Handler class name |
| ProcessedAtUtc | DateTime | When processed |

Unique index: `(EventId, HandlerName)` — prevents duplicate processing

## Dead-letter (messaging schema, shared)
| Field | Type |
|-------|------|
| Id | Guid |
| EventId | Guid |
| EventType | string |
| Payload | string |
| SourceModule | string |
| FailedAtUtc | DateTime |
| RetryCount | int |
| LastError | string |
| StackTrace | string? |
| Status | Dead / Reprocessed |

## Processing Flow
```
1. API saves business data + OutboxMessage in same transaction
2. Worker polls OutboxMessages (Status=Pending or Failed with NextRetryAtUtc <= UtcNow)
3. Worker locks row (LockedBy, LockedUntilUtc)
4. Worker deserializes event, resolves handlers from DI
5. Each handler checks Inbox before processing
6. On success: mark Inbox + mark Outbox as Processed
7. On failure: increment RetryCount, calculate NextRetryAtUtc (exponential backoff)
8. If RetryCount > MaxRetryCount: move to DeadLetterMessages
```

## Retry Policy
NextRetryAtUtc = UtcNow + 2^retryCount × 10 seconds
Max retries: 5 (configurable)
