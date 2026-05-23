# Outbox and Inbox

## Overview

The system uses database-backed reliable internal messaging.

API requests save business data and OutboxMessages in the same transaction. Worker processes OutboxMessages and dispatches integration events to in-process handlers. Consumer modules use InboxMessages for idempotency.

## Main Flow

1. An aggregate raises a Domain Event.
2. The Application layer maps the Domain Event to an Integration Event when cross-module communication is needed.
3. The producer module saves an OutboxMessage in its own schema in the same transaction as business data.
4. `EnglishTutor.Worker` discovers module outbox tables through registered `IModuleOutboxStore` entries and leases pending outbox messages with `FOR UPDATE SKIP LOCKED` inside a transaction.
5. Worker sets `Status=Processing`, `LockedBy`, and `LockedUntilUtc` before dispatching outside the lease transaction.
6. Worker delegates single-message dispatch to `OutboxMessageDispatcher`, which deserializes the integration event and publishes it through `IEventBus`.
7. Each consumer checks its Inbox.
8. If not processed, the handler executes and marks the InboxMessage as processed.
9. The producer outbox message is marked processed.

## Modules Involved

- Producer module Infrastructure: owns producer OutboxMessages.
- Consumer module Infrastructure/Application: owns InboxMessages and event handlers.
- `EnglishTutor.Worker`: processes outbox messages.
- `EnglishTutor.BuildingBlocks.EventBus`: dispatch abstraction.
- `EnglishTutor.BuildingBlocks.Outbox`: Outbox/Inbox/DeadLetter abstractions.

## Contracts Used

- `IIntegrationEvent`
- `IntegrationEvent`
- `IIntegrationEventHandler<TEvent>`
- `IEventBus`
- `OutboxMessage`
- `InboxMessage`
- `DeadLetterMessage`
- `IModuleOutboxStore`

## Events Published/Consumed

Concrete events are documented in `docs/events/integration-events.md`.

Current producers:

- Auth publishes user registration events.
- Users publishes profile, language, target-language, and level events.
- Vocabulary publishes review, mastery, and pronunciation events.
- Speaking publishes session and correction events.
- Mistakes publishes mistake created/reviewed/mastered events.
- AdminReports publishes progress-summary-ready events after weekly/monthly report generation.

Current consumers:

- Users consumes Auth registration events.
- Mistakes consumes Speaking correction and Vocabulary pronunciation events.
- Progress consumes Vocabulary, Speaking, and Mistakes events.
- `EnglishTutor.Worker` scans registered module outbox stores for `auth`, `users`, `vocabulary`, `speaking`, `mistakes`, `studyplans`, `learningcontent`, `exercises`, `assessments`, and `adminreports`, then writes exhausted failures to `messaging.DeadLetterMessages`.

## Read Models/Projections Updated

Progress and Mistakes projections are updated by consumer handlers through Integration Events. Aggregate dashboards should read these module-owned projections instead of live cross-module joins.

## Failure/Retry Behavior

- Event delivery is at-least-once.
- Handlers must be idempotent.
- Failed messages increment retry count and set `NextRetryAtUtc`.
- Retry uses exponential backoff.
- Messages exceeding retry limits are moved to `messaging.DeadLetterMessages` and their next retry timestamp is moved out to prevent repeated dead-letter inserts.
- `messaging.DeadLetterMessages.EventId` is unique so a lease/retry race cannot persist duplicate dead-letter rows for the same integration event.
- In-process event dispatch creates a DI scope per published event so scoped handlers and DbContexts are resolved safely.
- Single-message dispatch behavior is unit-tested without requiring PostgreSQL; full database scanning remains an EF Core Worker responsibility.
