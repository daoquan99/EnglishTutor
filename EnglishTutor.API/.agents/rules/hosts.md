# Hosts & runtime rules

Two runtime hosts. Different responsibilities. Don't blur them.

## EnglishTutor.Api

Stateless HTTP host.

Responsibilities:
- HTTP endpoints (minimal API + Presentation projects).
- Authentication (JWT bearer).
- Authorization (permission-based, see `authorization.md`).
- Request validation (FluentValidation pipeline behavior).
- MediatR dispatch (commands / queries).
- Database writes inside command handlers.
- `OutboxMessage` rows written in the **same transaction** as business data.
- SignalR hubs for realtime UI push (notifications).

Constraints:
- No in-memory state that affects business behavior. Caches must be Redis-backed or rebuilt on demand.
- No long-running operations in request handlers. Hand off to the Worker via outbox or queued job.
- Stateless to support horizontal scaling.

## EnglishTutor.Worker

Background processor host.

Responsibilities:
- Outbox processing (`OutboxProcessingJob`) — leases rows with `FOR UPDATE SKIP LOCKED`, dispatches integration events via in-process EventBus, marks processed / retries / dead-letters.
- Scheduled jobs (Quartz): study reminders, missed-session detection, weekly/monthly report generation, projection rebuild.
- Async AI calls when needed.
- Email / push notification dispatch when notification module produces work.

Constraints:
- Horizontal-scale safe — every shared resource access uses row-level leasing (`LockedBy` / `LockedUntilUtc`).
- Jobs are idempotent — at-least-once execution.
- Job failure: increment retry, exponential backoff, dead-letter after `MaxRetries`. Preserve error context for diagnosis.

## EnglishTutor.AppHost

.NET Aspire orchestrator. Local-dev only.

- Wires Postgres, Redis, API, Worker.
- Reads secrets from `dotnet user-secrets` on the AppHost project (`Parameters:jwt-secret`, `Parameters:seed-admin-password`).
- Boots the Aspire dashboard for logs / traces / metrics.

Don't put production wiring in AppHost. It's a developer convenience.

## Forbidden

- Running long-running work in API request handlers (use Worker).
- Dispatching integration events directly from API in-memory threads (use Outbox).
- Storing per-instance in-memory state in the API host.
- Bypassing the row-leasing pattern in the Worker.
