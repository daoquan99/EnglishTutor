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
7. Storage is selected through `Storage:Provider`; Local stores files under `Storage:LocalPath`, while S3/R2 uses signed HTTP requests against the configured bucket endpoint.
8. If `SeedData:Enabled=true`, API startup seeds AI routing/prompt defaults and baseline Vocabulary content.
9. In local Development, `Database:AutoMigrate=false` is the default so API/Worker startup does not mask missing EF migrations.
10. Local Development serves Swagger UI at `/swagger` and local storage files under `Storage:BaseUrl` when `Storage:Provider=Local`.
11. `EnglishTutor.AppHost` can orchestrate Postgres, Redis, API, and Worker through .NET Aspire for local development.

## Detailed Steps

- API requests receive or create an `X-Correlation-Id`.
- Unhandled exceptions are returned as `ProblemDetails`.
- Expected handler outcomes are returned through the Result response envelope.
- Worker schedules the outbox job on the configured interval.
- The outbox job scans registered `IModuleOutboxStore` entries for `auth`, `users`, `vocabulary`, `speaking`, `mistakes`, `studyplans`, `learningcontent`, `exercises`, and `assessments` outbox tables.
- Events are dispatched through the in-process event bus.
- Consumers use module-owned Inbox tables for idempotency.
- Local test database cleanup is available through `scripts/Test.ps1` or `scripts/Clear-TestDatabases.ps1`; cleanup only drops databases matching `english_tutor_test_%` unless `-IncludeDevelopmentDatabase` is explicitly passed.
- Local storage creates unique file keys and returns local URLs. S3/R2 storage signs `GET`, `PUT`, `HEAD`, and `DELETE` requests with AWS Signature V4.
- Seed data is disabled by default and skips existing records when enabled.
- API request logging writes method, path, status code, elapsed time, correlation ID, user ID, IP address, and user agent to Serilog.
- Swagger UI persists Bearer authorization and exposes `/api/auth/register` or `/api/auth/login` responses as the easiest way to copy an `accessToken`.
- To run the full local stack with Aspire, use `dotnet run --project src/Bootstrapper/EnglishTutor.AppHost/EnglishTutor.AppHost.csproj`.
- Aspire is optional for production architecture; it is a local orchestration and observability layer. It gives one dashboard for API/Worker logs, resource health, generated connection strings, container lifecycle, and service startup ordering.
- To run without Aspire, start Postgres/Redis through `docker compose up -d`, then run `EnglishTutor.Api` and `EnglishTutor.Worker` separately.
- `Database:AutoMigrate=true` is available as an opt-in local helper only after module migrations are in sync; if EF reports pending model changes, add the missing module migration first.

## Modules Involved

- `EnglishTutor.Api`
- `EnglishTutor.Worker`
- `EnglishTutor.AppHost`
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
