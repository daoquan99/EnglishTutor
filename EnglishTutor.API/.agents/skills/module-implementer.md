---
name: module-implementer
description: Builds a new module or adds features/endpoints to an existing module following Clean Architecture + DDD conventions. Use when adding commands, queries, entities, aggregates, or endpoints.
---

# Module Implementer

## When to invoke

- Adding a new module.
- Adding a new feature (command/query/endpoint) to an existing module.
- Adding an aggregate root, entity, value object, or business rule.

## Inputs

- Module name (e.g., `Vocabulary`).
- Feature description (e.g., "endpoint to mark a card as mastered").
- Any inputs/outputs and authorization requirements.

## Step-by-step

### 1. Read the module spec

Open `../modules/{module}.md`. Confirm schema, aggregate roots, existing events, contracts. If the feature crosses modules, also read the relevant `../workflows/{workflow}.md`.

### 2. Domain layer (`src/Modules/{Module}/EnglishTutor.Modules.{Module}.Domain/`)

- Pick or create the aggregate folder: `Domain/{AggregateRoot}/`.
- Aggregate root inherits `AggregateRoot<TId>` (gets soft-delete + domain events).
- Child entities inherit `Entity<TId>` (audit fields).
- Value objects → `ValueObjects/`. Enums → `Enums/`.
- Business rules → `Rules/` implementing `IBusinessRule`. Use `CheckRule(...)` in aggregate methods.
- Domain events → `Events/`. Raise via `AddDomainEvent(...)`.
- No infrastructure dependencies in Domain. No `Result`/`Error`.
- Audit fields are set by interceptor — do not set manually.

### 3. Application layer (`{Module}.Application`)

- Command/Query in `Commands/{Action}/` or `Queries/{Action}/` with `{Action}Command.cs`, `{Action}CommandHandler.cs`, `{Action}CommandValidator.cs`, response DTO.
- Handler signature: `Task<Result<TResponse>> Handle(TCommand, CancellationToken)`.
- Use `IXxxRepository` (aggregate roots only), `IDateTimeProvider`, `ICurrentUser` from BuildingBlocks/Application.
- Validator: FluentValidation rules — single source of truth for request validation.
- Module error catalog: `{Module}.Application/Shared/Errors/{Module}Errors.cs`. Add new error codes here.
- Map domain events to integration events in event handlers under `EventHandlers/` if cross-module impact.
- If consuming integration events: `EventHandlers/{EventName}Handler.cs` — check Inbox first.

### 4. Infrastructure layer (`{Module}.Infrastructure`)

- DbContext stays in `Persistence/{Module}DbContext.cs`. Add `DbSet<T>` if new aggregate.
- EF configuration: `Persistence/Configurations/{Aggregate}Configuration.cs` — mapping only.
- Schema: pin all tables to the module's schema via `ToTable("name", "schema")`.
- Repository: `Persistence/Repositories/{Aggregate}Repository.cs` implementing the Application interface.
- Contract Reader implementation if exposing data to other modules: `Persistence/Readers/{Name}Reader.cs`.
- Generate migration: see `../rules/database.md`.

### 5. Presentation layer (`{Module}.Presentation`)

- Endpoint in `{Module}Endpoints.cs` (minimal API) or a thin controller.
- Pattern: parse → dispatch via MediatR → map `Result` → HTTP. No business logic.
- Apply permission attribute / extension: `.RequirePermission(Permissions.{Module}.{Action})`.
- Map `Result.Failure(Error)` to HTTP via the project `ResultExtensions`.

### 6. Contracts (`{Module}.Contracts`)

- Add DTO classes that other modules will consume.
- Add integration event records if the command publishes one.
- Add Contract Reader interface if exposing data.
- Add permission constants if introducing new ones (Auth.Contracts only).
- Never add EF entities, Domain entities, or `IQueryable`.

### 7. DI registration

- Add new repos / readers / handlers via `{Module}.Infrastructure/DependencyInjection.cs` or the module's existing registration extension.

### 8. Tests

- Domain unit tests under `tests/EnglishTutor.Modules.{Module}.UnitTests/Domain/`.
- Handler tests under `.../Application/` with fake repos.
- Integration tests in `tests/EnglishTutor.IntegrationTests/` for DB-touching shapes.

### 9. Docs

- API endpoint → `docs/api/{module}.md` (template in `../rules/documentation.md`).
- Integration event → `docs/events/integration-events.md`.
- Workflow if cross-module → `docs/workflows/{workflow}.md`.

## Commands

```bash
cd EnglishTutor.API
dotnet build EnglishTutor.slnx
dotnet test tests/EnglishTutor.Modules.{Module}.UnitTests
dotnet test tests/EnglishTutor.ArchitectureTests
```

## Done when

- All four layers updated and DI wired.
- `dotnet build` + module unit tests + architecture tests pass.
- Migration generated (if schema changed) and named meaningfully.
- Docs updated for any new API / event / workflow surface.
- New error codes registered in the module's Application error catalog.
