# Architecture rules

Modular monolith. Clean Architecture per module. DDD tactical patterns. See `CLAUDE.md` and `README.md` for the big picture; this file is the enforcement reference.

## Module isolation

A module = `{Module}.Domain` + `.Application` + `.Infrastructure` + `.Presentation` + `.Contracts`.

- A module's Domain/Application/Infrastructure/Presentation must **never** reference another module's same layers.
- A module may reference **only** the target module's `Contracts` project.
- Forbidden across module boundaries: cross-module DbContext access, EF joins, navigation properties, `IQueryable` leaks, exposing EF entities through Contracts.
- Architecture tests in `tests/EnglishTutor.ArchitectureTests` enforce this. Run them after any structural change.

## Layering per module

```
Presentation -> Application -> Domain
Infrastructure -> Application
Infrastructure -> Domain
```

- Domain has zero dependencies on Application/Infrastructure/Presentation.
- Application has zero dependency on Infrastructure/Presentation.
- Presentation is thin: parse → dispatch MediatR command/query → map `Result` to HTTP.
- No business logic in controllers, minimal API endpoints, EF configurations, or DI files.
- EF `IEntityTypeConfiguration<T>` files are for mapping only.

## Cross-module communication (exactly three forms)

1. **Contract Readers** — sync small reads. Interface in `{Owner}.Contracts`, implementation in `{Owner}.Infrastructure`, return DTOs only. Never expose `IQueryable` or EF entities.
2. **Integration Events** — async state changes. Domain raises a `DomainEvent`; Application maps it to an `IntegrationEvent` and writes an `OutboxMessage` in the **same DB transaction** as business data. See `events.md`.
3. **Read Models / Projections** — owned by the **consuming** module, kept in its own schema, updated via integration events. Optimized for queries. Database views are for `AdminReports`/analytics only.

## Contracts project

- Contains: public DTOs, integration event contracts, Contract Reader interfaces, permission code constants (for Auth.Contracts).
- Must not contain: Domain entities, EF entities, `IQueryable`, repository abstractions.

## SharedKernel

- `BuildingBlocks.SharedKernel` stays small. Only truly cross-cutting concepts (base IDs, common value object primitives, time abstractions interfaces).
- Do not move module domain concepts into SharedKernel to "share" them.
- Do not move Application `Result`/`Error` types into SharedKernel or Domain.

## Hosts

- **`EnglishTutor.Api`** — stateless. HTTP, auth, validation, command/query dispatch, OutboxMessage writes. Horizontal-scale safe.
- **`EnglishTutor.Worker`** — outbox processing (Quartz `OutboxProcessingJob`), scheduled jobs (study reminders, missed-session detection, report generation, projection rebuild). Long-running work belongs here, never in request handlers.

## Forbidden

- Direct module-to-module database joins.
- Exposing `IQueryable` across modules.
- Accessing another module's `DbContext`.
- Referencing another module's `Infrastructure`/`Domain`/`Application`/`Presentation`.
- Calling AI providers outside `AI.Infrastructure`.
- Adding RabbitMQ/Kafka. Internal messaging is in-process EventBus + DB Outbox/Inbox.
- Adding `EventHandlerExecutions` table unless explicitly requested.
- Introducing multi-tenancy for language (see `multi-language.md`).
