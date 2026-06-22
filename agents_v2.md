# EnglishTutor Backend — Codex Agent Instructions

This repository is a production-oriented backend for a private-first AI English speaking tutor. Treat these instructions as the highest-priority project rules when coding with Codex.

## Product Context

The product helps users practice English through topic-first learning units. A user chooses a Topic first, then chooses a practice activity inside that topic:

- Free Talk
- Role-play
- Shadowing
- Translate Coach
- Quick Response
- Listening Challenge
- Correction Drill

The backend must support dynamic topic/mode/scenario configuration, AI provider/model/key routing, API key rotation, user quota, RabbitMQ-based asynchronous workflows, and production-grade security.


## Backend-Only Scope

This pack is for the backend only. Do not add client application implementation details, client framework instructions, component libraries, routing, styling, or UI tasks here. Any mention of a caller must stay generic, such as "client application", "API caller", or "SignalR client".

## Additional Architecture Influences

Favor pragmatic .NET architecture: rich domain where it creates value, vertical slices for application use cases, module contracts for immediate decisions, integration events for asynchronous workflows, and explicit boundaries between modules. Do not cargo-cult patterns; every pattern must protect a real boundary, consistency rule, security rule, or operability need.

## Phase 1 Target

Phase 1 is not a toy MVP. Build the backend as a production-ready modular monolith with RabbitMQ, worker processing, outbox/inbox, observability, secure API key management, and clear module boundaries from the beginning.

Phase 1 must include:

- .NET API host
- Worker host
- Modular Monolith architecture
- Each module as a mini Clean Architecture
- PostgreSQL primary database
- RabbitMQ + MassTransit
- Redis for cache/rate-limit/distributed locks
- SignalR for realtime session events
- JWT/refresh-token authentication
- Role/permission checks
- Encrypted AI API keys
- AI provider/model/key/routing admin APIs
- Topic/mode/scenario admin APIs
- Practice session lifecycle APIs
- Transcript persistence
- Feedback/correction background workflows
- Usage/quota tracking
- Audit logs
- Health checks
- Structured logging
- Automated tests
- ProblemDetails API error handling
- Resilience policies for provider calls and background work
- API documentation by backend module
- Architecture Decision Records
- AI provider prompt/template security
- Realtime session state machine
- Workflow/process manager for multi-step async flows
- Data privacy and retention policy
- Strongly typed options with startup validation
- Database performance/indexing rules
- PostgreSQL raw SQL/function/procedure governance with EF Core-first policy
- Root `Directory.Build.props` and `Directory.Packages.props` under solution folder `0. Solution Items`

## Required Architecture Style

Use:

- Modular Monolith
- Clean Architecture inside each module
- Vertical Slice/CQRS style use cases
- Module contracts for synchronous decisions
- Read models/snapshots for cross-module query needs
- Integration events for asynchronous communication
- Outbox pattern for reliable event publishing
- Inbox/idempotency for reliable event consumption
- RabbitMQ queue separation by workload type

Do not build microservices in Phase 1. Design module boundaries so that AiGateway, Feedback, Practice, or Worker responsibilities can be extracted later if needed.

## Project Structure Rule

Prefer this structure:

```text
src/
  EnglishTutor.Api/
  EnglishTutor.Worker/
  EnglishTutor.AppHost/                  optional Aspire/local orchestration

  BuildingBlocks/
    EnglishTutor.BuildingBlocks.Domain/
    EnglishTutor.BuildingBlocks.Application/
    EnglishTutor.BuildingBlocks.Infrastructure/
    EnglishTutor.BuildingBlocks.Contracts/

  Modules/
    Identity/
      EnglishTutor.Identity.Domain/
      EnglishTutor.Identity.Application/
      EnglishTutor.Identity.Infrastructure/
      EnglishTutor.Identity.Presentation/
      EnglishTutor.Identity.Contracts/

    Learning/
      EnglishTutor.Learning.Domain/
      EnglishTutor.Learning.Application/
      EnglishTutor.Learning.Infrastructure/
      EnglishTutor.Learning.Presentation/
      EnglishTutor.Learning.Contracts/

    Practice/
      EnglishTutor.Practice.Domain/
      EnglishTutor.Practice.Application/
      EnglishTutor.Practice.Infrastructure/
      EnglishTutor.Practice.Presentation/
      EnglishTutor.Practice.Contracts/

    Realtime/
      EnglishTutor.Realtime.Application/
      EnglishTutor.Realtime.Infrastructure/
      EnglishTutor.Realtime.Presentation/
      EnglishTutor.Realtime.Contracts/

    AiGateway/
      EnglishTutor.AiGateway.Domain/
      EnglishTutor.AiGateway.Application/
      EnglishTutor.AiGateway.Infrastructure/
      EnglishTutor.AiGateway.Presentation/
      EnglishTutor.AiGateway.Contracts/

    Feedback/
      EnglishTutor.Feedback.Domain/
      EnglishTutor.Feedback.Application/
      EnglishTutor.Feedback.Infrastructure/
      EnglishTutor.Feedback.Presentation/
      EnglishTutor.Feedback.Contracts/

    Progress/
      EnglishTutor.Progress.Domain/
      EnglishTutor.Progress.Application/
      EnglishTutor.Progress.Infrastructure/
      EnglishTutor.Progress.Presentation/
      EnglishTutor.Progress.Contracts/

    Quota/
      EnglishTutor.Quota.Domain/
      EnglishTutor.Quota.Application/
      EnglishTutor.Quota.Infrastructure/
      EnglishTutor.Quota.Presentation/
      EnglishTutor.Quota.Contracts/

    Audit/
      EnglishTutor.Audit.Domain/
      EnglishTutor.Audit.Application/
      EnglishTutor.Audit.Infrastructure/
      EnglishTutor.Audit.Presentation/
      EnglishTutor.Audit.Contracts/
```

## Solution Items and Shared Build Configuration

The backend solution must expose a Visual Studio solution folder named:

```text
0. Solution Items
```

This folder must include root-level repository/build files:

- `Directory.Build.props`
- `Directory.Packages.props`
- `.editorconfig`
- `AGENTS.md`
- `README.md`
- `global.json` if used
- `NuGet.config` if used

`Directory.Packages.props` is the single source for NuGet versions. `.csproj` files must use versionless package references unless a documented exception exists.

`Directory.Build.props` is the single source for common build/analyzer settings. It must not contain secrets or package versions.

## Dependency Rules

Allowed:

- `Api` references module `Presentation` and `Infrastructure` registration extension methods.
- `Worker` references module `Infrastructure` and consumer registration extension methods.
- A module may reference another module's `.Contracts` project only.
- Application layer may reference BuildingBlocks.Application and BuildingBlocks.Contracts.
- Domain layer may reference BuildingBlocks.Domain only.
- Infrastructure layer may reference Application and Domain of its own module.

Forbidden:

- Module A directly referencing Module B Infrastructure.
- Module A directly using Module B DbContext.
- Module A directly querying Module B tables.
- Sharing EF entities across modules.
- Generic repository everywhere by default.
- Logging secrets, API keys, Authorization headers, raw connection strings, or raw audio unless explicitly configured.

## Module Communication Rules

Use this decision matrix:

| Need | Mechanism |
|---|---|
| Query reference data from another module | Local read model/snapshot maintained by events |
| Need a decision immediately and consistency matters | Synchronous module contract/facade |
| Side-effect, long-running work, progress update, AI job | Integration event + outbox + RabbitMQ + worker |
| Security/permission check | Claims + permission version, or Identity contract for sensitive operations |
| API key quota/rotation/cooldown | AiGateway contract with reserve/confirm/release lease |
| User daily/session quota | Quota contract with reserve/confirm/cancel reservation |

## Backend Stack

Default Phase 1 choices:

- Runtime: .NET LTS
- API: ASP.NET Core Minimal APIs or Controllers, keep style consistent
- Realtime: SignalR
- DB: PostgreSQL
- ORM: EF Core per module DbContext
- Provider: Npgsql.EntityFrameworkCore.PostgreSQL
- Queries: EF Core first; Dapper allowed for complex read/reporting queries; stored procedures discouraged and allowed only with documented justification
- Messaging: MassTransit + RabbitMQ
- Cache/locks/rate limiting: Redis
- Auth: JWT access token + refresh token rotation
- Encryption: ASP.NET Core Data Protection or a dedicated encryption service
- Logging: Serilog structured logs
- Observability: OpenTelemetry-ready traces/metrics/logs
- Validation: FluentValidation
- Mapping: Mapster or explicit mapping; do not overuse AutoMapper
- Testing: xUnit, FluentAssertions, Testcontainers for PostgreSQL/RabbitMQ/Redis

## Naming Rules

Use clear domain names:

- Topic, ModeDefinition, TopicMode, Scenario
- PracticeSession, TranscriptMessage, SessionEvent, SessionReview
- AIProvider, AIModel, AIProviderKey, AIRoutingRule, AIRouteLease
- UsageLog, UserQuotaRule, ApiKeyQuotaState
- Correction, CorrectionItem, MistakePattern, ExtractedVocabulary
- AuditLog, SecurityEvent

Use PascalCase for C# types, camelCase for JSON, and lower_snake_case for PostgreSQL schema/table/column names unless an ADR says otherwise.

## Auditable Entities and Soft Delete Rules

All domain entities must include audit metadata:

- `CreatedAtUtc`
- `CreatedUserId`
- `UpdatedAtUtc`
- `UpdatedUserId`

All aggregate roots must include audit metadata plus soft-delete metadata:

- `IsDeleted`
- `DeletedAtUtc`
- `DeletedUserId`

Rules:

- Aggregate roots inherit audit fields from the base Entity type.
- Only aggregate roots own soft-delete lifecycle fields by default.
- Child entities should not have `IsDeleted` unless they have an independent lifecycle and a documented reason.
- API request DTOs must never accept or set audit fields directly.
- Application handlers must not manually set audit fields.
- Audit fields must be populated by Infrastructure, preferably through an EF Core SaveChanges interceptor.
- Soft delete must go through a domain method such as `MarkDeleted(...)`.
- Directly setting `IsDeleted`, `DeletedAtUtc`, or `DeletedUserId` outside the aggregate root is forbidden.
- Queries and repositories must exclude soft-deleted aggregate roots by default.
- Including deleted aggregate roots requires an explicit admin/internal use case.
- Hard delete is forbidden by default and allowed only for retention, cleanup, or internal maintenance jobs with documentation.
- Migrations must include audit and soft-delete columns where applicable.
- Tests must cover create, update, soft delete, query filtering, and hard-delete restrictions.

## PostgreSQL Raw SQL / Function / Procedure Policy

PostgreSQL functions/procedures and provider-specific raw SQL are allowed but discouraged. Do not use them for normal CRUD, business/domain rules, workflow orchestration, permission checks, AI routing, quota decisions, or cross-module communication.

Default order for data access:

```text
EF Core DbContext/LINQ
  -> optimized EF Core projection/compiled query
  -> Dapper/raw SQL in module Infrastructure
  -> PostgreSQL function/procedure only with documented justification
```

Provider-specific SQL files must live in the owning module Infrastructure project:

```text
src/Modules/{Module}/EnglishTutor.{Module}.Infrastructure/Persistence/PostgreSql/
```

PostgreSQL SQL artifacts must be applied by module migrations or a documented deployment script. They must have integration tests, a justification, and an entry in `docs/database/postgresql-functions-procedures.md`. If they affect an API, update the owning module API docs.

Application and Domain layers must never call provider-specific SQL directly. SQL artifact names must not leak through module Contracts.

## Security Rules

- Never send provider API keys to any client application.
- Never return full saved API keys from the API.
- Store API keys encrypted at rest.
- Show only masked key previews.
- Never log secrets.
- Use RBAC for admin endpoints.
- Require Owner role for API key management.
- Require Admin/Owner for provider/model/routing/topic management.
- Use audit logs for sensitive changes.
- Add rate limits for login, token refresh, practice start, and AI calls.
- Default to not storing raw audio.

## Domain/Event/Job Skills

When a task touches domain entities, aggregate roots, auditing, soft delete, background processing, queues, DDD, repositories, event contracts, outbox, inbox, integration events, or module contracts, read the matching skill in `.agents/skills/` before editing code.

The single `massTransit-rabbitmq-outbox` skill is not enough by itself; use it together with the more specific skills added for domain modeling, rich domain behavior, event-driven design, shared contracts, idempotency, background jobs, auditable entities, and soft-delete aggregate roots.

## Task Phase Review Gate Rules

Every backend task must be divided into task phases and executed one phase at a time.

Standard task phases:

```text
Phase 0 — Understanding & Impact Analysis
Phase 1 — Design & Contract Plan
Phase 2 — Implementation
Phase 3 — Tests, Docs & Quality Gates
Phase 4 — Review Package
```

After completing each task phase, stop and wait for explicit user review/approval before continuing to the next phase. Do not silently jump to the next phase. Read `plans/TASK_EXECUTION_GATES.md` and `.agents/rules/32-phase-gated-task-execution-rules.md` before executing any task.

## How Codex Should Work

When implementing tasks:

1. Read `plans/PHASE-1-BE-PRODUCTION.md` first.
2. Read the relevant task file under `plans/phase-1/`.
3. Read `.agents/rules/*` and `.agents/architectures/*` related to the task.
4. Use `.agents/skills/*/SKILL.md` when the task matches the skill description.
5. Execute only one approved task phase at a time.
6. Keep module boundaries intact.
7. Add tests for the slice.
8. Run build/tests before finalizing.
9. Stop after each task phase and wait for user review before continuing.
10. Document any intentional deviation in the task notes.

## AI/Realtime/Workflow Hardening Rules

- AI provider prompts must be versioned and protected from prompt injection.
- AI provider outputs must be validated before persistence.
- Realtime practice sessions must use explicit state transitions and idempotent terminal commands.
- Multi-step async workflows must use persisted workflow/job state or a process manager.
- Transcripts and provider payloads are private data; logs are redacted by default.
- Retention and cleanup settings must be configurable and validated.
- New list/read APIs must be paginated and indexed for the expected query path.

## Preferred Build Commands

Use these as defaults once the solution exists:

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```

If Docker Compose exists:

```bash
docker compose up -d postgres rabbitmq redis
dotnet test
```

## Definition of Done for Backend Tasks

A task is done only when:

- API/application behavior is implemented.
- Module boundaries are respected.
- Migrations are added when schema changes.
- Integration events are defined when cross-module changes matter.
- Outbox/inbox/idempotency are considered.
- Logs are structured and safe.
- Sensitive data is protected.
- Unit/integration tests exist where meaningful.
- Health/observability impact is considered.
- Public contracts are documented.
- Audit fields and soft-delete behavior are implemented or explicitly considered for affected entities/aggregate roots.


## API Documentation Rule

Every backend API endpoint created, updated, removed, or versioned must update the owning module API document under:

```text
docs/api/modules/{ModuleName}.md
```

Documentation must include method/route, purpose, permission, request, response, error responses, events emitted, background jobs triggered, audit behavior, and idempotency behavior.

Updating code or DTOs without updating the module API document is incomplete.

## Testing Database Rule

Tests must never use development or production databases. If a test creates a database, it must be temporary and must be removed/dropped after the test run.

Preferred testing approaches:

- Testcontainers PostgreSQL/RabbitMQ/Redis disposed after the test assembly.
- Unique temporary PostgreSQL database names with teardown calling `EnsureDeleted` or DROP DATABASE.

Tests must not leave databases, schemas, queues, or Redis keys behind.

## Additional Required Skills

For all backend production-hardening tasks, Codex must also use:

- `security-hardening`
- `resilience-fault-tolerance`
- `observability`
- `testing-strategy-dotnet`
- `api-design-error-handling`
- `database-migration-data-ownership`
- `caching-redis-rate-limit`
- `architecture-decision-records`
- `module-dependency-rules`
- `api-documentation`
- `ai-provider-prompt-security`
- `realtime-session-state-machine`
- `workflow-process-manager`
- `data-privacy-retention`
- `dotnet-options-configuration`
- `database-performance-indexing`
- `efcore-postgresql`
- `postgresql-raw-sql-procedure`
- `phase-gated-task-execution`
- `dotnet-solution-items-msbuild-props`
- `auditable-entities-soft-delete`