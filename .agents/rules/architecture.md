# Architecture Rules

## Architecture Type

```
Production Modular Monolith
+ Clean Architecture per Module
+ DDD tactical patterns per Module
+ DbContext per Module
+ Schema per Module
+ Contracts for synchronous cross-module reads
+ Integration Events for state changes
+ Transactional Outbox per producer module
+ Inbox per consumer module for idempotency
+ Retry + Dead-letter for event failures
+ Read Models / Projections for aggregate screens
+ API Host + Worker Host
+ Architecture Tests to enforce boundaries
```

## Module Structure

Each module follows this structure:

```
src/Modules/{ModuleName}/
├── {ModuleName}.Domain
├── {ModuleName}.Application
├── {ModuleName}.Infrastructure
├── {ModuleName}.Presentation
└── {ModuleName}.Contracts
```

## Layer Dependencies (Clean Architecture)

```
Presentation → Application → Domain
Infrastructure → Application
Infrastructure → Domain
```

- Domain must NOT depend on Application, Infrastructure, or Presentation.
- Application must NOT depend on Infrastructure or Presentation.
- Infrastructure implements Application abstractions.
- Presentation is thin and delegates to Application commands/queries.

## Domain Error Boundary

- Domain protects invariants with `DomainException` / `BusinessRuleValidationException`.
- Domain may use `IBusinessRule` for named business rules.
- Domain must NOT use Application `Result`, `Result<T>`, or `Error`.
- Domain must NOT reference `EnglishTutor.BuildingBlocks.Application`.
- Domain must NOT define module response error records such as `AuthError` only to carry `Code` and `Message`.
- Module error catalogs with stable error codes belong in the module Application layer.
- Application maps expected use-case failures to `BuildingBlocks.Application.Results.Error`.
- Presentation maps Application `Result/Error` to HTTP responses.
- Global exception middleware maps `DomainException` / `BusinessRuleValidationException` to `ProblemDetails`.

## Module Isolation Rules

1. Each module owns its own DbContext.
2. Each module owns its own database schema.
3. Each module owns its own migrations.
4. A module must NOT access another module's DbContext.
5. A module must NOT reference another module's Domain/Application/Infrastructure/Presentation.
6. A module may reference ONLY another module's Contracts project.
7. Do NOT expose IQueryable across module boundaries.
8. Do NOT expose Domain Entities through Contracts.
9. Do NOT use cross-module navigation properties.
10. Do NOT perform cross-module EF joins in core business logic.

## Cross-Module Communication (Allowed Methods)

### 1. Contract Readers (synchronous reads)
- Return DTO/read model only.
- Interface lives in owner module's Contracts project.
- Implementation lives in owner module's Infrastructure.
- Used when data is needed immediately.

### 2. Integration Events (state changes)
- Event names must be past tense.
- Events represent business facts.
- Use Outbox for reliable publishing.
- Consumers must be idempotent via Inbox.

### 3. Read Models / Projections (aggregate screens)
- The module that needs the read model owns it.
- Read models may duplicate data from other modules.
- Updated through Integration Events.

### 4. Database Views (admin/reporting ONLY)
- Allowed ONLY for AdminReports module.
- NOT allowed for core business flows.

## Runtime Topology

```
EnglishTutor.Api (HTTP, stateless, horizontally scalable)
EnglishTutor.Worker (Outbox processing, scheduled jobs, notifications)
```

- API instances do NOT dispatch events directly. They write OutboxMessages.
- Worker reads OutboxMessages and dispatches to handlers.

## Event Messaging

```
In-process Event Dispatcher
+ Transactional Outbox per producer module
+ Inbox per consumer module
+ Retry with exponential backoff
+ Dead-letter table (messaging.DeadLetterMessages)
+ Worker host processes outbox
```

- NO RabbitMQ/Kafka at the beginning.
- At-least-once delivery + idempotent handlers.
- Domain Events are internal to a module.
- Integration Events are public for other modules.
- Flow: Aggregate → DomainEvent → Application maps to IntegrationEvent → saved to Outbox.

## Database Strategy

- 1 physical PostgreSQL database.
- Multiple schemas (one per module + messaging).
- Each module has its own DbContext and migration stream.
- Cross-module references use IDs only, no FK navigation properties.

## Schemas

```
auth, users, studyplans, learningcontent, vocabulary,
exercises, speaking, ai, mistakes, assessments,
progress, notifications, adminreports, messaging
```

## AI Module Isolation

- AI provider clients (Gemini, Gemma) MUST exist only in AI.Infrastructure.
- Other modules use AI.Contracts interfaces only.
- Prompt templates live in AI module, NOT in calling modules.

## Multi-Language Design

- Language is NOT tenant. No multi-tenancy for language.
- User language settings: NativeLanguageCode, UiLanguageCode, ExplanationLanguageCode, ActiveTargetLanguageCode.
- Progress scoped by UserId + TargetLanguageCode.
- Speaking sessions store language snapshot at start.

## Horizontal Scaling

- API must be stateless.
- OutboxMessages enable event delivery independent of API instance.
- Worker uses DB row locking for horizontal scaling.
