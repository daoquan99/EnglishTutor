# Agent Skills

## Agent 1: Architecture Guardian

**Role:** Enforce architecture boundaries and maintain architectural integrity.

**Skills:**
- Validate module boundary compliance (no cross-module DbContext, no IQueryable leaks, no cross-infrastructure references)
- Write and maintain architecture tests using NetArchTest / ArchUnitNET
- Verify Clean Architecture layer dependencies per module
- Review code changes for architecture violations
- Maintain ADR documents in `docs/adr/`
- Validate that Contracts projects expose only DTOs/read models, never Domain Entities
- Enforce AI client isolation (only in AI.Infrastructure)
- Validate Outbox/Inbox usage patterns
- Verify centralized build/package management compliance

**When to invoke:**
- After every module implementation or cross-module change
- When adding new project references
- When adding new integration events or contract readers
- During code review

**Output locations:**
- `docs/adr/` — Architecture Decision Records
- `tests/EnglishTutor.ArchitectureTests/` — Architecture test project

---

## Agent 2: Module Implementer

**Role:** Implement modules following Clean Architecture + DDD per AGENTS.md.

**Skills:**
- Create Domain layer: Entities, Value Objects, Aggregates, Domain Events, Business Rules
- Create Application layer: Commands, Queries, Handlers, Validators (FluentValidation), DTOs
- Create Infrastructure layer: DbContext, EF Configurations, Migrations, Repositories, Contract Readers
- Create Presentation layer: Minimal API endpoints (thin, delegate to Application)
- Create Contracts project: DTOs, Read Models, Integration Events, Contract Interfaces
- Wire DI registration per module
- Implement Result pattern for all handlers
- Keep expected use-case error catalogs in Application using `BuildingBlocks.Application.Results.Error`
- Use `DomainException` / `BusinessRuleValidationException` / `IBusinessRule` for domain invariant failures
- Never reference Application `Result/Error` from Domain
- Follow existing code style and naming conventions
- Check `Directory.Build.props` and `Directory.Packages.props` before adding packages

**When to invoke:**
- When building a new module from scratch
- When adding new features to an existing module
- When adding new endpoints, commands, queries, or entities

**Output locations:**
- `src/Modules/{ModuleName}/` — Module source code
- `.agents/modules/{module-name}.md` — Module specification updates

---

## Agent 3: Test Writer

**Role:** Write unit, integration, and architecture tests.

**Skills:**
- Unit tests for domain logic (entities, value objects, business rules) — no mocking infrastructure
- Handler tests for application commands/queries — mock repositories
- Integration tests for database queries and important module flows
- Architecture tests for dependency rules using NetArchTest
- Validator tests for FluentValidation rules
- Event handler tests with mocked Inbox/repositories
- Ensure `dotnet build && dotnet test` passes

**When to invoke:**
- After domain logic implementation
- After handler implementation
- After adding new architecture rules
- Before marking any task as complete

**Output locations:**
- `tests/EnglishTutor.Modules.{Module}.UnitTests/`
- `tests/EnglishTutor.IntegrationTests/`
- `tests/EnglishTutor.ArchitectureTests/`

---

## Agent 4: Documentation Agent

**Role:** Create and maintain all project documentation.

**Skills:**
- API docs: endpoint, purpose, auth, request/response body, validation rules, error codes, flow, related modules, events
- Workflow docs: overview, flow, steps, modules involved, contracts, events, read models, failure behavior
- Event docs: event name, producer, consumers, payload, when published, side effects, idempotency
- ADR docs: context, decision, consequences
- Module specification docs
- Keep docs in sync with code changes

**When to invoke:**
- After adding/changing API endpoints
- After adding/changing integration events
- After adding/changing business workflows
- After adding/changing read models or contracts

**Output locations:**
- `docs/api/{module}.md` — API documentation
- `docs/workflows/{workflow}.md` — Workflow documentation
- `docs/events/integration-events.md` — Event catalog
- `docs/events/outbox-inbox.md` — Messaging documentation
- `docs/adr/` — Architecture Decision Records
- `.agents/documents/` — Agent-level documentation
- `.agents/workflows/` — Agent-level workflow specs

---

## Agent 5: Integration & Events Agent

**Role:** Implement cross-module communication via Outbox/Inbox, events, and read model projections.

**Skills:**
- Implement Outbox message saving in producer modules (same transaction as business data)
- Implement Inbox idempotency checks in consumer modules
- Implement Integration Event handlers (check inbox → process → mark inbox)
- Map Domain Events to Integration Events
- Implement Read Model / Projection updates via events
- Implement Dead-letter handling (move failed events after max retries)
- Wire Worker host jobs: OutboxProcessingJob, MissedSessionDetectionJob, ReportGenerationJobs
- Configure Quartz scheduled jobs in Worker
- Implement retry with exponential backoff
- Test full Outbox → Worker → Inbox pipeline end-to-end

**When to invoke:**
- When wiring events between modules
- When implementing event handlers in consumer modules
- When adding Worker background jobs
- When implementing read model projections
- When debugging event delivery issues

**Output locations:**
- `src/Modules/{Module}/Application/EventHandlers/` — Event handlers
- `src/Bootstrapper/EnglishTutor.Worker/Jobs/` — Worker jobs
- `src/BuildingBlocks/EnglishTutor.BuildingBlocks.Outbox/` — Outbox abstractions
- `src/BuildingBlocks/EnglishTutor.BuildingBlocks.EventBus/` — Event bus
