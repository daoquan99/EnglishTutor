# Foundation Runtime Workflow

## Overview

Phase 1 establishes the runtime foundation for the modular monolith: API host, Worker host, health checks, internal messaging abstractions, and local infrastructure.

## Main Flow

1. `EnglishTutor.Api` starts as the HTTP host.
2. API registers health checks, authentication/authorization, CORS, Swagger, exception handling, correlation IDs, and application pipeline behaviors.
3. `GET /health` verifies that the API host responds.
4. `EnglishTutor.Worker` starts as the background host.
5. Worker registers Quartz and the outbox processing job.
6. Worker processes module outbox tables and dispatches integration events to Inbox-protected consumers.

## Detailed Steps

- API requests receive or create an `X-Correlation-Id`.
- Unhandled exceptions are returned as `ProblemDetails`.
- Expected handler outcomes are returned through the Result response envelope.
- Worker schedules the outbox job on the configured interval.
- The outbox job scans `auth`, `users`, `vocabulary`, `speaking`, and `mistakes` outbox tables.
- Events are dispatched through the in-process event bus.
- Consumers use module-owned Inbox tables for idempotency.

## Modules Involved

- `EnglishTutor.Api`
- `EnglishTutor.Worker`
- `EnglishTutor.BuildingBlocks.Application`
- `EnglishTutor.BuildingBlocks.Domain`
- `EnglishTutor.BuildingBlocks.Infrastructure`
- `EnglishTutor.BuildingBlocks.EventBus`
- `EnglishTutor.BuildingBlocks.Outbox`

## Contracts Used

- Application command/query abstractions.
- Result/Error response abstractions.
- Integration event abstractions.
- Outbox/Inbox abstractions.

## Events Published/Consumed

None in Phase 1.

## Read Models/Projections Updated

None in Phase 1.

## Failure/Retry Behavior

- API uses global exception handling for unexpected failures.
- Worker job execution is non-concurrent.
- Failed outbox messages increment retry count and use exponential backoff.
- Messages that exceed retry limits are copied to `messaging.DeadLetterMessages`.
