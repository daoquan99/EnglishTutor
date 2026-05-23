# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository layout

This repo holds two deployable apps at the root, sharing nothing at build time:

- `EnglishTutor.API/` — .NET 10 modular monolith backend. Contains `src/` (Bootstrapper, BuildingBlocks, Modules), `tests/`, `Directory.Build.props`, `Directory.Packages.props`, `global.json`, and `EnglishTutor.slnx`. **All `dotnet` commands must be run from inside `EnglishTutor.API/`** — `global.json` pins the SDK there, and the slnx and Central Package Management files live there.
- `EnglishTutor.Web/` — Next.js 16 (App Router) + React 19 frontend. pnpm-managed.
- `scripts/` — PowerShell helpers (`Reset-DevDatabase.ps1`, `Clear-TestDatabases.ps1`, `Test.ps1`). `Test.ps1` runs `dotnet test EnglishTutor.API/EnglishTutor.slnx` with optional DB cleanup.
- `README.md` at the repo root is the deep-dive backend reference (architecture diagrams, module map, DB schemas, AI/Outbox flow). Use it when you need detail beyond this file.

## Common commands

### Backend (run from `EnglishTutor.API/`)

```bash
# Build full solution
dotnet build EnglishTutor.slnx

# Test full solution
dotnet test EnglishTutor.slnx

# Test a single module (replace Vocabulary)
dotnet test tests/EnglishTutor.Modules.Vocabulary.UnitTests

# Test a single test method
dotnet test tests/EnglishTutor.Modules.Vocabulary.UnitTests --filter "FullyQualifiedName~MarkMasteredCommandHandlerTests.Should_Succeed_When_Valid"

# Architecture rule checks (NetArchTest)
dotnet test tests/EnglishTutor.ArchitectureTests

# Run everything via .NET Aspire (recommended for local dev — boots Postgres, Redis, API, Worker)
dotnet run --project src/Bootstrapper/EnglishTutor.AppHost

# Run API or Worker standalone (requires Postgres + Redis already running, e.g. via docker compose)
dotnet run --project src/Bootstrapper/EnglishTutor.Api
dotnet run --project src/Bootstrapper/EnglishTutor.Worker

# Add a migration for a specific module (replace Vocabulary)
dotnet ef migrations add <Name> \
  --project src/Modules/Vocabulary/EnglishTutor.Modules.Vocabulary.Infrastructure \
  --startup-project src/Bootstrapper/EnglishTutor.Api \
  --context VocabularyDbContext \
  --output-dir Persistence/Migrations
```

Migrations live at `Persistence/Migrations/` inside each module's `.Infrastructure` project — pass `--output-dir Persistence/Migrations` so new migrations land in the right place.

The Worker's outbox/messaging schema migrations live in `src/Bootstrapper/EnglishTutor.Worker/Outbox/Migrations/` and use `MessagingDbContext` (startup project = `EnglishTutor.Worker`).

### Frontend (run from `EnglishTutor.Web/`)

```bash
pnpm dev          # Next.js dev server (turbopack, HTTPS via local certs in ./certificates)
pnpm build
pnpm start
pnpm lint
pnpm typecheck    # tsc --noEmit
```

### Local infra & scripts (run from repo root)

```bash
docker compose up -d                 # Postgres 17, Redis 7, PgAdmin, RedisInsight
./scripts/Reset-DevDatabase.ps1      # drop volumes and recreate fresh DB
./scripts/Test.ps1                   # clears test DBs then runs full backend test suite
./scripts/Test.ps1 -SkipDatabaseCleanup
```

## Backend architecture — non-obvious invariants

The big picture (modular monolith, Clean Arch per module, Outbox/Inbox) is in `README.md`. The rules below are easy to violate and not visible from a single file:

**Module isolation.** A module's `Domain`/`Application`/`Infrastructure`/`Presentation` layers must not reference any other module — only the target module's `Contracts` project. Forbidden specifically: cross-module DbContext access, cross-module EF joins, cross-module navigation properties, `IQueryable` leaking across module boundaries, exposing EF entities through Contracts. Architecture tests in `tests/EnglishTutor.ArchitectureTests` enforce this — run them after structural changes.

**Cross-module communication has exactly three forms:**
1. **Contract Readers** (sync small reads) — interface in target module's `Contracts`, implementation in `Infrastructure`, returns DTOs only.
2. **Integration Events** (async state changes) — Domain raises a `DomainEvent`; Application maps it to an `IntegrationEvent` and writes an `OutboxMessage` **in the same DB transaction** as the business data. Never publish events from request memory.
3. **Read Models / Projections** — owned by the consuming module, updated via integration events.

**Outbox/Inbox is mandatory** for any integration event. The Worker leases rows with `FOR UPDATE SKIP LOCKED`, publishes to the in-process EventBus, and consumers must check `messaging.InboxMessages` before processing. Handlers must be idempotent (at-least-once delivery). Failed events past retry land in `messaging.DeadLetterMessages`.

**AI module exclusivity.** No module outside `AI.Infrastructure` may call Gemini/OpenAI/DeepSeek directly. Other modules consume `AI.Contracts` only. Prompt templates, model routing, cost/token logging, and rate limits all live in the `AI` module.

**Domain vs Application error handling.**
- Domain uses `DomainException` / `BusinessRuleValidationException` (via `IBusinessRule`) for invariant violations only. Domain must not reference `BuildingBlocks.Application` or use `Result`/`Error`.
- Application uses `Result`/`Result<T>` + module-local error catalogs (e.g. `AuthErrors`, `VocabularyErrors`) under `{Module}.Application/Shared/Errors/`. Presentation maps these to HTTP responses.

**UTC + naming.** All persisted timestamps and DTO/event timestamps are UTC and named with the `Utc` suffix (`CreatedAtUtc`, `OccurredOnUtc`, `ExpiresAtUtc`, …). Never use `DateTime.Now`. Application code uses `IDateTimeProvider.UtcNow`. If a workflow needs a calendar day for a user, compute the boundary in Application from their timezone setting and persist the resulting business date — the underlying timestamp stays UTC.

**Audit + soft-delete are interceptor-driven.** `Entity<TId>` implements `IAuditableEntity` (`CreatedAtUtc`, `CreatedByUserId`, `UpdatedAtUtc`, `UpdatedByUserId`); `AggregateRoot<TId>` implements `ISoftDeletable` and owns domain events. Audit fields are populated by EF `SaveChanges` interception — don't set them manually in handlers. Soft delete is enforced by EF query filters; never add `IsActive` flags to base entity types.

**Authorization is permission-based, not role-based.** Check permission codes (e.g. `ai.providers.manage`). `admin.full_access` is the wildcard for the seeded admin. Permission constants live in `Auth.Contracts` so other modules may reference them. Never gate on role names like `"Admin"` at runtime.

**Multi-language is not multi-tenancy.** Users module owns language settings (`NativeLanguageCode`, `UiLanguageCode`, `ExplanationLanguageCode`, `TargetLanguageCode`). Progress, mistakes, mastery, etc. are scoped by `UserId + TargetLanguageCode`. Speaking sessions snapshot language at session start. Do not introduce a tenant column.

**Database per module.** One physical Postgres DB `english_tutor_db` with 14 schemas (`auth`, `users`, `studyplans`, `learningcontent`, `vocabulary`, `exercises`, `speaking`, `ai`, `mistakes`, `assessments`, `progress`, `notifications`, `adminreports`, `messaging`). Each module owns its DbContext + schema + migration history. DB views are allowed for `AdminReports`/analytics only — never for core business flows.

**Hosts.** `EnglishTutor.Api` is stateless (HTTP, validation, command/query dispatch, OutboxMessage writes). `EnglishTutor.Worker` runs outbox processing, Quartz scheduled jobs (study reminders, missed-session detection, report generation), and projection rebuilds. Long-running work belongs in the Worker, not request handlers.

**Central package management.** Add NuGet versions only to `EnglishTutor.API/Directory.Packages.props`. `.csproj` files use bare `<PackageReference Include="Name" />` (no `Version` attribute). Common build properties (TargetFramework, Nullable, ImplicitUsings) come from `Directory.Build.props` — don't redeclare them per-project.

**Aggregate folder layout.** Domain code is organized per aggregate root:
```
Domain/{AggregateRoot}/
├── {AggregateRoot}.cs        # inherits AggregateRoot<TId>
├── Entities/                 # child entities, inherit Entity<TId>
├── ValueObjects/
├── Enums/
├── Rules/                    # IBusinessRule implementations
├── Events/                   # domain events
└── Services/                 # pure domain services (no repo/DbContext/HTTP)
```
Repositories exist only for aggregate roots. Domain services must not touch repositories, DbContext, cache, HTTP, AI providers, current user, or any infrastructure.

## Frontend architecture — non-obvious invariants

Detailed conventions are in `EnglishTutor.Web/AGENTS.md`. Key points:

- **Stack:** Next.js App Router, React 19, TypeScript strict, Tailwind v4, shadcn/ui, TanStack Query (server state), Zustand (client state), react-hook-form + Zod (forms), Recharts. Real-time uses `@microsoft/signalr`. **Do not add another UI framework** (MUI/AntD/Chakra/Mantine/daisyUI).
- **Folder layout:** `src/app` for routes only (no business logic), `src/features/{feature}` for business modules mirroring backend bounded contexts, `src/shared` for cross-feature primitives. Feature module shape: `api/`, `components/`, `hooks/`, `schemas/`, `types/`, `utils/`, `index.ts`.
- **Route groups:** `(public)`, `(auth)`, `(app)`, `(admin)`.
- **API client:** centralized in `src/shared/api` (`http-client.ts`, `api-error.ts`, `auth-token-store.ts`, `query-client.ts`). Per-feature API functions live in `features/{feature}/api`.
- **shadcn aliases** (from `components.json`): `@/shared/components`, `@/shared/components/ui`, `@/shared/lib/utils`, `@/shared/lib`, `@/shared/hooks`. Generated UI primitives go in `src/shared/components/ui` — keep them generic; do feature-specific composition in feature components.
- **Types match backend DTOs** but UI view models stay separate when shape diverges. Avoid `any`. Use Zod for form schemas and complex request validation.

## Documentation expectations

When changing public surface area, update the matching doc in the same task (do not defer):
- API endpoints (request/response, validation, errors, auth) → `docs/api/{module}.md`
- Integration events (producer, consumers, payload, idempotency) → `docs/events/integration-events.md`
- Cross-module workflows → `docs/workflows/{workflow}.md`
- Architecture decisions → `docs/adr/`

If a doc you need to update is not present on disk, ask before fabricating one.

## Definition of done

- `dotnet build EnglishTutor.slnx` and `dotnet test EnglishTutor.slnx` pass (from `EnglishTutor.API/`), or you explain why a run isn't possible.
- Architecture tests pass after any cross-module or layering change.
- Docs updated for any API/event/workflow/contract surface change.
- No new cross-module DbContext access, `IQueryable` leak, AI provider call outside `AI.Infrastructure`, role-based check, or local server time (`DateTime.Now`).
- No package version added to a `.csproj` file directly — versions live in `Directory.Packages.props` only.
