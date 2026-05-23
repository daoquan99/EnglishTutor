# AGENTS.md

## Project Type

This is a production-grade .NET modular monolith for an AI-powered English Tutor SaaS system.

The backend architecture is:

- Production Modular Monolith
- Clean Architecture per module
- DDD tactical patterns per module
- DbContext per module
- Schema per module
- Contracts for small synchronous cross-module reads
- Integration Events for state changes
- Transactional Outbox per producer module
- Inbox per consumer module for idempotency
- Retry + Dead-letter for failed event processing
- Read Models / Projections for aggregate screens
- API Host + Worker Host
- Architecture Tests to enforce boundaries

Do not redesign the architecture without asking.

---

## Agent Context Sources

Use repository guidance in this order:

1. `AGENTS.md` is the primary repo-wide instruction source.
2. `.agents/rules/` contains detailed rule references used to verify or clarify `AGENTS.md`.
3. `.agents/modules/` contains module-specific specifications and should be read before changing or reviewing a specific module.
4. `.agents/workflows/` contains cross-module business workflow specifications and should be read before changing workflows, events, projections, or multi-module behavior.
5. `.agents/documents/` contains supporting architecture, product, or implementation notes and should be used when a task references those topics or when extra background is needed.
6. `docs/` contains official project documentation that must be kept in sync with API, workflow, event, contract, read model, and architecture changes.

When to use each source:

- For a small local code change, use `AGENTS.md` and inspect the touched code.
- For module work, use `AGENTS.md` plus `.agents/modules/{module}.md` when present.
- For cross-module workflows, use `AGENTS.md`, the relevant `.agents/workflows/` file, and each affected module specification.
- For architecture review or dependency changes, use `AGENTS.md` plus `.agents/rules/architecture.md`.
- For coding, testing, package, or documentation conventions, use `AGENTS.md` plus `.agents/rules/rules.md` when clarification is needed.
- For agent role responsibilities, use `AGENTS.md` plus `.agents/rules/skills.md` when planning complex work.
- For API changes, update the related file in `docs/api`.
- For workflow changes, update the related file in `docs/workflows`.
- For integration event changes, update `docs/events`.
- For ADR-level architecture decisions, update `docs/adr`.

Conflict resolution:

- If `AGENTS.md` conflicts with `.agents/rules`, `.agents/modules`, `.agents/workflows`, or `.agents/documents`, follow `AGENTS.md` and report the conflict.
- If implementation plans conflict with the architecture in `AGENTS.md`, stop and ask before changing the architecture.
- If expected behavior is unclear, ask for clarification before changing APIs, contracts, events, workflows, or architecture decisions.

---

## Module List

Core modules:

- Auth
- Users
- StudyPlans
- LearningContent
- Vocabulary
- Exercises
- Speaking
- AI
- Mistakes
- Assessments
- Progress
- Notifications
- AdminReports

Optional/future modules must not be added unless explicitly requested.

---

## Module Structure

Each module should follow this structure:

```text
src/Modules/{ModuleName}
- {ModuleName}.Domain
- {ModuleName}.Application
- {ModuleName}.Infrastructure
- {ModuleName}.Presentation
- {ModuleName}.Contracts
```

Notes:

- `{ModuleName}.Presentation` contains module endpoints/controllers only.
- `{ModuleName}.Contracts` contains only public contracts, DTOs, read models, and integration event contracts.
- Domain entities must not be exposed through Contracts.

---

## Architecture Rules

- Follow Modular Monolith architecture.
- Each module must be isolated.
- Each module owns its own DbContext.
- Each module owns its own database schema.
- Each module owns its own migrations.
- Do not reference another module's Infrastructure layer.
- Do not reference another module's Domain/Application/Presentation layer.
- A module may reference another module's Contracts project only.
- Do not access another module's DbContext directly.
- Do not perform direct module-to-module EF joins.
- Do not add cross-module navigation properties.
- Do not expose IQueryable across module boundaries.
- Do not expose EF Core entities from APIs or Contracts.
- Prefer explicit DTOs/read models over exposing entities.
- Do not introduce shared domain models across modules.
- Do not put all code into SharedKernel.
- SharedKernel must stay small and contain only truly shared concepts.

Allowed cross-module communication:

1. Module Contracts for small synchronous reads
2. Read Models / Projections for aggregate screens
3. Integration Events for state changes

Contract Reader rules:

- Contract Reader interfaces live in the owner module's Contracts project.
- Contract Reader implementations live in the owner module's Infrastructure project.
- Contract Readers must return DTOs/read models only.
- Contract Readers are for small synchronous reads when data is needed immediately.
- Do not expose IQueryable or EF entities through Contract Readers.

Integration Event rules for cross-module state changes:

- Integration Events are public module contracts.
- Domain Events are internal to a module.
- Flow: Aggregate -> DomainEvent -> Application maps to IntegrationEvent -> OutboxMessage.

Read Model / Projection rules for aggregate screens:

- The module that needs the read model owns it.
- Read models may duplicate data from other modules.
- Read models should be updated through Integration Events.
- Database views are allowed only for AdminReports/reporting/analytics, not core business flows.

---

## Layering Rules

Each module should follow Clean Architecture:

```text
Presentation -> Application -> Domain
Infrastructure -> Application
Infrastructure -> Domain
```

Rules:

- Domain must not depend on Application, Infrastructure, or Presentation.
- Application must not depend on Infrastructure or Presentation.
- Infrastructure may implement Application abstractions.
- Presentation must be thin and delegate to Application commands/queries.
- Domain logic must stay inside Domain or Application layer.
- Do not put business logic in Controllers, Minimal API endpoints, EF Core configurations, or dependency injection files.
- EF Core configurations are for mapping only.

---

## Domain Modeling Rules

Entity and Aggregate Root boundaries must be explicit:

- Aggregate roots inherit `AggregateRoot<TId>`.
- Non-root domain entities inherit `Entity<TId>`.
- Implemented module domains should be organized by aggregate root folder.
- Aggregate folder shape should be:

```text
Domain
└── {AggregateRootName}
    ├── {AggregateRootName}.cs
    ├── Entities
    ├── ValueObjects
    ├── Enums
    ├── Rules
    ├── Events
    └── Services
```

- Repositories should be created for aggregate roots only.
- Child entities should be changed through their aggregate root, not through standalone repositories.
- If a child entity needs its own repository and independent lifecycle, consider making it an aggregate root.
- Domain events belong to the aggregate that raises them.
- Aggregate-specific events, value objects, enums, rules, and domain services should stay near that aggregate when the module is refactored into aggregate folders.
- Domain services are only for pure domain logic that does not naturally belong to an entity, aggregate, or value object.
- Domain services must not call repositories, DbContext, cache, HTTP, AI providers, current user, or infrastructure services.

Audit and soft-delete rules:

- `Entity<TId>` implements `IAuditableEntity` and owns `CreatedAtUtc`, `CreatedByUserId`, `UpdatedAtUtc`, and `UpdatedByUserId`.
- `AggregateRoot<TId>` owns domain events and implements `ISoftDeletable`.
- `ISoftDeletable` is the standard soft-delete interface name.
- Important non-root entities may implement `ISoftDeletable` only when they have a real independent soft-delete lifecycle.
- Do not add `IsActive` to base entity types; model business state with aggregate-specific status/enums.
- Audit fields are set by EF SaveChanges interception, not manually in application handlers.
- Soft delete is enforced by EF SaveChanges interception and query filters for `ISoftDeletable`.
- Domain logic may still set business timestamps when those timestamps are part of events or business behavior.

Date/time rules:

- All persisted timestamps must be UTC.
- All API contracts, DTOs, integration events, domain events, and read models should expose timestamps as UTC.
- Timestamp property names must use the `Utc` suffix, for example `CreatedAtUtc`, `UpdatedAtUtc`, `DeletedAtUtc`, `ExpiresAtUtc`, `OccurredOnUtc`, `StartedAtUtc`, and `CompletedAtUtc`.
- Do not use or persist local server time.
- Do not use `DateTime.Now` in production code.
- Prefer the existing `IDateTimeProvider.UtcNow` abstraction in Application and Infrastructure code that needs the current time.
- Domain methods that need current time should receive a UTC `DateTime` from the caller when practical; otherwise they must use `DateTime.UtcNow`, never local time.
- Frontend clients are responsible for converting UTC timestamps to the user's display timezone.
- If a workflow needs a user's calendar day/week/month, calculate the timezone boundary in Application using the user's timezone/language settings and persist the resulting business date/snapshot explicitly. The underlying timestamp remains UTC.

---

## Domain Error Handling Rules

Domain must protect invariants and model business rules. It must not model API/application response errors.

Rules:

- Domain must not reference `EnglishTutor.BuildingBlocks.Application`.
- Domain must not use `Result`, `Result<T>`, or `Error` from the Application layer.
- Domain must not create module-specific response error records such as `AuthError` only to carry `Code` and `Message`.
- Domain invariant violations should use `DomainException` or `BusinessRuleValidationException`.
- Use `IBusinessRule` when a domain rule has a meaningful business name and can be expressed independently.
- `IBusinessRule` describes the broken rule; `BusinessRuleValidationException` reports that the rule was broken.
- Domain exceptions are guardrails for invalid entity/value-object state, not normal use-case control flow.

Application owns expected use-case outcomes:

- Application handlers return `Result` / `Result<T>` for expected failures such as not found, invalid credentials, conflicts, or validation failures.
- Module-specific error catalogs such as `AuthErrors` belong in `{Module}.Application`, not `{Module}.Domain`.
- Application error catalogs should use `EnglishTutor.BuildingBlocks.Application.Results.Error`.
- Presentation maps `Result/Error` to normal HTTP responses.
- Global exception handling maps `DomainException` / `BusinessRuleValidationException` to `ProblemDetails`.

---

## Application Rules

- Application layer exposes Commands and Queries via MediatR.
- Use async/await for IO.
- Use CancellationToken in application handlers and infrastructure calls.
- Use Result pattern consistently with existing project conventions for expected application outcomes.
- Use module-specific Application error catalogs for expected use-case failures that need stable error codes.
- Use meaningful exceptions only for exceptional failures, invalid domain invariants, or infrastructure failures that cannot be represented as normal application results.
- Keep handlers focused.
- Do not create large god services.
- Do not add unnecessary abstractions.
- Do not create generic repositories unless the project already uses them.
- Avoid anemic command handlers that push all logic into infrastructure.

## Authorization Rules

- Authorization checks must be permission-based, not role-based.
- Roles are only groups of permissions.
- Check access through permission codes such as `ai.providers.manage`.
- `admin.full_access` is the wildcard permission for the seeded full-admin account.
- Runtime access decisions must use permission claims, not role names such as `Admin`.
- Permission code constants live in `Auth.Contracts` so other modules may reference them through the allowed Contracts boundary.

---

## Database Rules

- Use one physical PostgreSQL database.
- Use multiple schemas: one schema per module plus `messaging`.
- Each module has its own DbContext.
- Each module uses its own schema.
- Migration history should be schema-specific where possible.
- Do not add foreign key navigation properties across modules.
- Cross-module references should use IDs or value objects only.
- Within the same module, normal FK relationships are allowed.
- Do not query another module's tables directly from a module.
- Do not create cross-module database views for core business flows.
- Database views/materialized views are allowed only for AdminReports/reporting/analytics.

Schema names:

```text
auth, users, studyplans, learningcontent, vocabulary,
exercises, speaking, ai, mistakes, assessments,
progress, notifications, adminreports, messaging
```

---

## Integration Events and Messaging Rules

The system uses internal reliable messaging:

- In-process Event Dispatcher
- Transactional Outbox per producer module
- Inbox per consumer module
- Retry with exponential backoff
- Dead-letter messages in `messaging.DeadLetterMessages`

Rules:

- Do not publish integration events directly from request memory without Outbox.
- Producer modules save integration events into their own Outbox.
- Worker host processes Outbox messages.
- Consumer modules use Inbox for idempotency.
- Event delivery is at-least-once.
- Event handlers must be idempotent.
- Do not assume exactly-once delivery.
- Event names should be past tense, e.g. `SpeakingSessionCompletedIntegrationEvent`.
- Domain entities should raise Domain Events, not Integration Events.
- Application layer maps Domain Events to Integration Events when needed.
- Important integration events must be saved as OutboxMessages in the same transaction as business data.
- Consumer handlers must check Inbox before processing and mark Inbox after successful processing.
- Do not add EventHandlerExecutions unless explicitly requested.
- Use DeadLetterMessages for events that exceed retry limits.
- Failed messages must preserve enough error context for diagnosis and possible reprocessing.

---

## API Host and Worker Host Rules

The system has two runtime hosts:

- `EnglishTutor.Api`
- `EnglishTutor.Worker`

`EnglishTutor.Api` responsibilities:

- HTTP APIs
- Authentication/authorization
- Request validation
- Execute commands/queries
- Save business data
- Save OutboxMessages
- Stay stateless for horizontal scaling

`EnglishTutor.Worker` responsibilities:

- Outbox processing
- Integration event dispatch
- Notifications
- Scheduled jobs
- Projection rebuild jobs
- Weekly/monthly reports
- Async AI jobs if needed

Do not put long-running background jobs inside request handlers.

Horizontal scaling rules:

- `EnglishTutor.Api` must stay stateless.
- API instances do not dispatch integration events directly from request memory.
- API instances write OutboxMessages; Worker instances process OutboxMessages.
- Worker horizontal scaling must use database row locking/leasing.
- Outbox row-locking fields should include `LockedBy`, `LockedUntilUtc`, `RetryCount`, and `NextRetryAtUtc`.

---

## AI Module Rules

The AI module owns:

- Gemini/Gemma clients
- Model routing
- Prompt templates
- Prompt versions
- AI request logs
- AI usage counters
- AI cost estimation
- AI quota/rate-limit logic

Other modules must use `AI.Contracts` only.

Forbidden:

- Do not call Gemini/Gemma clients outside AI.Infrastructure.
- Do not put prompt templates inside Speaking, Vocabulary, Exercises, or Assessments.
- Do not duplicate AI model routing logic in other modules.

---

## Multi-language Rules

The system supports:

- NativeLanguageCode
- UiLanguageCode
- ExplanationLanguageCode
- TargetLanguageCode

Rules:

- Language is not tenant.
- Do not introduce multi-tenancy for language.
- Users module owns user language settings.
- Progress should be scoped by `UserId + TargetLanguageCode`.
- Speaking sessions must store language snapshot at session start.
- Vocabulary content should support translations and example translations.
- LearningContent should support translations for lessons/conversations.
- AI requests must include native, target, and explanation language context where relevant.

---

## Progress and Attempt Tracking Rules

Progress module stores summaries and aggregates, not every detailed attempt.

Owner modules store detailed attempts:

- Vocabulary owns vocabulary review attempts, pronunciation attempts, example sentence attempts, and vocabulary mastery.
- Exercises owns exercise attempts, answers, and exercise results.
- Speaking owns speaking turns, turn results, and session summaries.
- Assessments owns assessment attempts, answers, grading results, and rubrics.
- Mistakes owns user mistakes and mistake review state.
- Progress owns EXP, activity logs, skill progress, streaks, daily/weekly/monthly progress, and dashboard snapshots.

Do not put all detailed learning attempts into Progress.

---

## Read Model Rules

Use Read Models / Projections for aggregate screens such as:

- Dashboard
- Learning path
- Vocabulary study cards
- Speaking history
- Admin reports

Rules:

- The module that needs the read model owns it.
- Read models may duplicate data from other modules.
- Read models should be updated through Integration Events.
- Read models are optimized for queries.
- Do not use IQueryable from other modules to build aggregate screens live.

---

## API Documentation Rules

Whenever an API endpoint is added, removed, or changed, the agent must update the related documentation in the same task.

API documentation lives in:

```text
docs/api
```

Workflow documentation lives in:

```text
docs/workflows
```

Event documentation lives in:

```text
docs/events
```

Rules:

- Do not add or update an API without updating its documentation.
- Do not change request/response contracts without updating API docs.
- Do not change validation rules without updating API docs.
- Do not change authentication/authorization behavior without updating API docs.
- Do not add new integration events without updating event docs.
- Do not change existing integration event payloads without updating event docs.
- Do not change a business workflow without updating workflow docs.
- Do not change read models/projections without updating workflow docs.
- Do not change module communication without updating architecture/workflow docs.
- If an API change affects frontend behavior, document the expected request/response and error cases clearly.
- If an API change affects events, list produced/consumed events.
- If an API change affects Progress, Notifications, Mistakes, Assessments, or AdminReports, update the related workflow docs.
- Documentation updates are part of the Definition of Done.
- If the expected behavior is unclear, stop and ask for clarification before implementing the API change.

Every API document should include:

```text
- Endpoint
- Purpose
- Auth requirement
- Request body
- Response body
- Validation rules
- Error codes
- Application flow
- Related modules
- Integration events produced
- Integration events consumed, if applicable
- Read models/projections updated, if applicable
```

Every workflow document should include:

```text
- Overview
- Main flow
- Detailed steps
- Modules involved
- Contracts used
- Events published/consumed
- Read models/projections updated
- Failure/retry behavior if relevant
```

Every event document should include:

```text
- Event name
- Producer module
- Consumer modules
- Payload fields
- When it is published
- Side effects
- Idempotency notes
- Related workflows
```

---

## Documentation Structure Rules

Recommended documentation structure:

```text
docs
├── api
│   ├── auth.md
│   ├── users.md
│   ├── study-plans.md
│   ├── learning-content.md
│   ├── vocabulary.md
│   ├── exercises.md
│   ├── speaking.md
│   ├── ai.md
│   ├── mistakes.md
│   ├── assessments.md
│   ├── progress.md
│   ├── notifications.md
│   └── admin-reports.md
│
├── workflows
│   ├── speaking-session.md
│   ├── vocabulary-flashcard-review.md
│   ├── exercise-completion.md
│   ├── level-up-assessment.md
│   ├── study-reminder.md
│   └── progress-dashboard.md
│
├── events
│   ├── integration-events.md
│   └── outbox-inbox.md
│
└── adr
    ├── 0001-modular-monolith.md
    ├── 0002-module-owned-dbcontext.md
    ├── 0003-cross-module-communication.md
    └── 0004-internal-messaging-outbox-inbox.md
```

When adding a new module, add or update the related API, workflow, event, and ADR documents when applicable.

---

---

## Centralized Build and Package Management Rules

The backend solution must use centralized build and package management.

Required root-level files:

```text
Directory.Build.props
Directory.Packages.props
```

### Directory.Build.props

`Directory.Build.props` is the single place for common build settings across the backend solution.

It should define shared project settings such as:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <GenerateDocumentationFile>false</GenerateDocumentationFile>
  </PropertyGroup>
</Project>
```

Rules:

- Do not duplicate common build properties in individual `.csproj` files.
- Do not set `TargetFramework`, `Nullable`, `ImplicitUsings`, or `LangVersion` repeatedly in module projects unless there is a specific approved reason.
- All backend projects should inherit common build settings from `Directory.Build.props`.
- If a project requires a different build setting, explain why before changing it.

### Directory.Packages.props

`Directory.Packages.props` is the single place for NuGet package versions across the backend solution.

It should use Central Package Management:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="..." />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="..." />
    <PackageVersion Include="FluentValidation" Version="..." />
  </ItemGroup>
</Project>
```

Rules:

- Do not specify package versions directly in individual `.csproj` files.
- All package versions must be defined in `Directory.Packages.props`.
- Project files may reference packages using `<PackageReference Include="PackageName" />` only.
- When adding a new NuGet package, add its version to `Directory.Packages.props`.
- When updating a package version, update it only in `Directory.Packages.props`.
- Keep package versions consistent across the whole solution.
- Do not introduce duplicate package version definitions.

### Package Version Policy

- Use stable, modern, actively maintained packages.
- Prefer the latest stable package versions compatible with the solution target framework.
- Do not use preview, beta, rc, nightly, or deprecated packages unless explicitly approved.
- Before adding a new package, check whether the project already has an approved package for the same purpose.
- Do not add unnecessary packages for functionality already available in the .NET platform or existing dependencies.
- If a package choice is unclear, ask before adding it.
- If the latest stable version causes compatibility issues, use the newest compatible stable version and explain the reason.

### Project File Rules

Individual `.csproj` files should stay minimal.

Allowed:

```xml
<ItemGroup>
  <PackageReference Include="FluentValidation" />
</ItemGroup>
```

Forbidden:

```xml
<ItemGroup>
  <PackageReference Include="FluentValidation" Version="11.9.0" />
</ItemGroup>
```

Forbidden:

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

unless explicitly required and approved for that specific project.

### Agent Responsibilities

When adding or changing projects/packages, the agent must:

- Check `Directory.Build.props` first for common build settings.
- Check `Directory.Packages.props` first for existing package versions.
- Add new package versions only to `Directory.Packages.props`.
- Keep individual `.csproj` files free from package versions.
- Keep individual `.csproj` files free from duplicated common build settings.
- Mention any package additions or version changes in the task summary.

## Coding Rules

- Use the latest stable C# version used by the project.
- Follow existing code style and naming conventions.
- Keep methods small and readable.
- Prefer explicit names over vague abstractions.
- Do not rename public APIs unless required.
- Do not rewrite unrelated code.
- Keep changes minimal and focused.
- Reuse existing conventions.
- Add XML comments only when they add real value.
- Avoid premature optimization.
- Avoid over-engineering.

---

## Testing Rules

- Add unit tests for domain logic.
- Add handler tests for application logic where useful.
- Add integration tests for database queries and important module flows.
- Add architecture tests for dependency rules using NetArchTest / ArchUnitNET.
- Architecture tests must enforce Clean Architecture layer dependencies and module boundary rules.
- Do not skip failing tests.
- If tests cannot run, explain why.
- Before completing a task, run:

```bash
dotnet build
dotnet test
```

If the command cannot be run, explain the reason.

---

## Agent Role Rules

Use these role responsibilities when planning, implementing, reviewing, testing, and documenting work.

### Architecture Guardian

Responsibilities:

- Validate module boundary compliance.
- Verify no cross-module DbContext access, IQueryable leaks, cross-infrastructure references, or forbidden module references.
- Verify Clean Architecture layer dependencies per module.
- Maintain architecture tests using NetArchTest / ArchUnitNET.
- Validate Contracts projects expose only DTOs/read models/contracts, never Domain entities.
- Enforce AI client isolation in AI.Infrastructure.
- Validate Outbox/Inbox usage patterns.
- Verify centralized build and package management compliance.
- Maintain ADR documents in `docs/adr`.

Invoke after every module implementation, cross-module change, new project reference, new integration event, contract reader addition, or architecture review.

Output locations:

- `docs/adr`
- `tests/EnglishTutor.ArchitectureTests`

### Module Implementer

Responsibilities:

- Create Domain layer: entities, value objects, aggregates, domain events, and business rules.
- Create Application layer: commands, queries, handlers, validators, and DTOs.
- Create Infrastructure layer: DbContext, EF configurations, migrations, repositories when locally established, and Contract Reader implementations.
- Create Presentation layer endpoints/controllers that stay thin and delegate to Application.
- Create Contracts project DTOs, read models, integration event contracts, and contract interfaces.
- Wire DI registration per module.
- Implement Result pattern for handlers.
- Check `Directory.Build.props` and `Directory.Packages.props` before adding projects or packages.

Invoke when building a new module, adding module features, or adding endpoints, commands, queries, or entities.

Output locations:

- `src/Modules/{ModuleName}`
- `.agents/modules/{module-name}.md` when module specification updates are needed

### Test Writer

Responsibilities:

- Add unit tests for domain logic without mocking infrastructure.
- Add handler tests for application commands/queries.
- Add integration tests for database queries and important module flows.
- Add architecture tests for dependency rules.
- Add validator tests for FluentValidation rules.
- Add event handler tests with mocked Inbox/repositories where useful.
- Ensure `dotnet build` and `dotnet test` pass before completion.

Output locations:

- `tests/EnglishTutor.Modules.{Module}.UnitTests`
- `tests/EnglishTutor.IntegrationTests`
- `tests/EnglishTutor.ArchitectureTests`

### Documentation Agent

Responsibilities:

- Maintain API docs with endpoint, purpose, auth, request/response body, validation rules, error codes, flow, related modules, and events.
- Maintain workflow docs with overview, flow, steps, modules, contracts, events, read models, and failure behavior.
- Maintain event docs with producer, consumers, payload, publish timing, side effects, idempotency, and related workflows.
- Maintain ADR docs.
- Keep docs in sync with code changes.

Invoke after adding or changing API endpoints, integration events, business workflows, read models, or contracts.

Output locations:

- `docs/api/{module}.md`
- `docs/workflows/{workflow}.md`
- `docs/events/integration-events.md`
- `docs/events/outbox-inbox.md`
- `docs/adr`
- `.agents/documents`
- `.agents/workflows`

### Integration & Events Agent

Responsibilities:

- Implement Outbox message saving in producer modules in the same transaction as business data.
- Implement Inbox idempotency checks in consumer modules.
- Implement Integration Event handlers using check Inbox -> process -> mark Inbox.
- Map Domain Events to Integration Events.
- Implement Read Model / Projection updates via events.
- Implement retry and DeadLetterMessages for events that exceed retry limits.
- Wire Worker jobs such as `OutboxProcessingJob`, missed-session detection, and report generation jobs.
- Configure Quartz scheduled jobs in Worker.
- Test the Outbox -> Worker -> Inbox pipeline end to end.

Invoke when wiring cross-module events, implementing event consumers, adding Worker jobs, implementing projections, or debugging event delivery.

Output locations:

- `src/Modules/{Module}/Application/EventHandlers`
- `src/Bootstrapper/EnglishTutor.Worker/Jobs`
- `src/BuildingBlocks/EnglishTutor.BuildingBlocks.Outbox`
- `src/BuildingBlocks/EnglishTutor.BuildingBlocks.EventBus`

---

## Before Changing Code

Before making changes:

1. Inspect existing patterns.
2. Reuse existing conventions.
3. Explain the files you plan to modify.
4. Keep changes minimal and focused.
5. Do not change architecture decisions without asking.

---

## Definition of Done

A task is not complete unless all applicable items are done:

- Code compiles.
- Tests pass or the agent explains why they cannot run.
- New/updated APIs are documented in `docs/api`.
- New/updated workflows are documented in `docs/workflows`.
- New/updated integration events are documented in `docs/events`.
- Request/response DTO changes are reflected in docs.
- Validation rules are reflected in docs.
- Error codes are reflected in docs.
- Related read models/projections are documented.
- Related module contracts are documented when changed.
- Architecture rules are not violated.
- No cross-module DbContext access is introduced.
- No IQueryable is exposed across modules.
- No external AI provider is called outside AI.Infrastructure.
- No RabbitMQ/Kafka/EventHandlerExecutions/multi-tenancy is added unless explicitly requested.

When finishing a task, provide a summary with:

```text
- Changed files
- API changes
- Contract changes
- Event changes
- Read model/projection changes
- Docs updated
- Tests run
- Known limitations or follow-up tasks
```

---

## Forbidden

- Do not rewrite the whole project.
- Do not change architecture without asking.
- Do not introduce direct module-to-module database joins.
- Do not expose IQueryable across modules.
- Do not access another module's DbContext.
- Do not reference another module's Infrastructure.
- Do not put all code into SharedKernel.
- Do not move Application `Error` / `Result` concepts into Domain or SharedKernel to avoid module-local error catalogs.
- Do not create generic repositories unless the project already uses them.
- Do not call external AI providers outside AI.Infrastructure.
- Do not add RabbitMQ/Kafka unless explicitly requested.
- Do not add EventHandlerExecutions unless explicitly requested.
- Do not introduce multi-tenancy for language.
- Do not add or update APIs without updating related documentation.
- Do not specify NuGet package versions directly in `.csproj` files.
- Do not duplicate common build properties from `Directory.Build.props` in module projects.
- Do not add preview/beta/rc/nightly NuGet packages unless explicitly requested.
