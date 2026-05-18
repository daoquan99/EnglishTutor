# Phase 1: Foundation — Detailed Tasks

> **Goal:** Solution skeleton, BuildingBlocks, API Host, Worker Host, shared infrastructure, architecture tests, initial docs.
> **Dependencies:** None (starting point)
> **Estimated tasks:** 14

---

## Task 1.1: Create Solution & Centralized Build Configuration

**Agent:** Module Implementer

**Description:**
Create the .NET solution file and centralized build/package management files at the repository root.

**Files to create:**

```
d:\Projects\EnglishTutor\EnglishTutor.sln
d:\Projects\EnglishTutor\Directory.Build.props
d:\Projects\EnglishTutor\Directory.Packages.props
d:\Projects\EnglishTutor\.gitignore
d:\Projects\EnglishTutor\global.json (pin .NET SDK version)
```

**Directory.Build.props content:**

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

**Directory.Packages.props content:**
- `ManagePackageVersionsCentrally = true`
- Add initial packages: `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `MediatR`, `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`, `Swashbuckle.AspNetCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `Serilog`, `Serilog.Sinks.Console`, `Serilog.AspNetCore`, `Quartz`, `NetArchTest.Rules`, `xunit`, `Moq`, `FluentAssertions`, `Microsoft.NET.Test.Sdk`

**Acceptance Criteria:**
- [ ] `dotnet new sln` creates solution
- [ ] `Directory.Build.props` exists with shared settings
- [ ] `Directory.Packages.props` exists with all initial package versions
- [ ] `.gitignore` covers .NET, Rider, VS artifacts
- [ ] No individual `.csproj` specifies `TargetFramework` or package versions

---

## Task 1.2: Create API Host Project

**Agent:** Module Implementer

**Description:**
Create the `EnglishTutor.Api` web host project as the HTTP entry point.

**Files to create:**

```
src/Bootstrapper/EnglishTutor.Api/
├── EnglishTutor.Api.csproj
├── Program.cs
├── DependencyInjection.cs
├── appsettings.json
├── appsettings.Development.json
├── Middlewares/
│   ├── GlobalExceptionHandlerMiddleware.cs
│   └── CorrelationIdMiddleware.cs
└── OpenApi/
    └── SwaggerConfiguration.cs
```

**Program.cs responsibilities:**
- Configure Serilog with structured logging
- Register services via `DependencyInjection.AddApiServices()`
- Add Authentication (JWT Bearer) — placeholder config
- Add Authorization
- Add OpenAPI/Swagger
- Add CORS
- Map health check endpoint: `GET /health`
- Use `GlobalExceptionHandlerMiddleware`
- Use `CorrelationIdMiddleware`

**GlobalExceptionHandlerMiddleware:**
- Catch unhandled exceptions
- Return standardized `ProblemDetails` response
- Log exception with correlation ID
- Map `BusinessRuleValidationException` → 422
- Map `NotFoundException` → 404
- Map `UnauthorizedAccessException` → 401
- Map unknown → 500

**CorrelationIdMiddleware:**
- Read `X-Correlation-Id` header or generate new GUID
- Set on `HttpContext.Items` and response header
- Add to Serilog `LogContext`

**Acceptance Criteria:**
- [ ] `dotnet build` passes
- [ ] `dotnet run` starts and Swagger UI is accessible at `/swagger`
- [ ] `GET /health` returns 200
- [ ] Correlation ID flows through request/response
- [ ] Exceptions return ProblemDetails JSON

---

## Task 1.3: Create Worker Host Project

**Agent:** Module Implementer

**Description:**
Create the `EnglishTutor.Worker` background host for Outbox processing, scheduled jobs, and event dispatching.

**Files to create:**

```
src/Bootstrapper/EnglishTutor.Worker/
├── EnglishTutor.Worker.csproj
├── Program.cs
├── DependencyInjection.cs
├── appsettings.json
└── Jobs/
    └── OutboxProcessingJob.cs (placeholder)
```

**Program.cs responsibilities:**
- Configure as `IHost` (not web host)
- Configure Serilog
- Register Quartz for scheduled jobs
- Register `DependencyInjection.AddWorkerServices()`
- Placeholder for Outbox processing job registration

**OutboxProcessingJob (placeholder):**
- Implements `IJob` (Quartz)
- Log "Outbox processing started" on execute
- Will be wired to real `IOutboxProcessor` in Phase 4

**Acceptance Criteria:**
- [ ] `dotnet build` passes
- [ ] Worker starts and logs "Worker started"
- [ ] Quartz scheduler initializes
- [ ] Job placeholder runs on configured interval (every 5 seconds default)

---

## Task 1.4: Create BuildingBlocks.Domain

**Agent:** Module Implementer

**Description:**
Create shared domain abstractions used by all module Domain layers.

**Files to create:**

```
src/BuildingBlocks/EnglishTutor.BuildingBlocks.Domain/
├── EnglishTutor.BuildingBlocks.Domain.csproj
├── Entity.cs
├── AggregateRoot.cs
├── ValueObject.cs
├── DomainEvent.cs
├── IDomainEventHolder.cs
├── Exceptions/
│   ├── DomainException.cs
│   └── BusinessRuleValidationException.cs
└── Rules/
    └── IBusinessRule.cs
```

**Entity<TId>:**
- Generic base with `Id` property
- Override `Equals`, `GetHashCode` based on `Id`

**AggregateRoot<TId> : Entity<TId>:**
- Private `List<DomainEvent> _domainEvents`
- `IReadOnlyList<DomainEvent> DomainEvents` property
- `AddDomainEvent(DomainEvent)` protected method
- `ClearDomainEvents()` method

**ValueObject:**
- Abstract class with `GetEqualityComponents()` pattern
- Override `Equals`, `GetHashCode`, `==`, `!=`

**DomainEvent:**
- `Guid EventId` (auto-generated)
- `DateTime OccurredOnUtc`
- Implements `MediatR.INotification`

**IBusinessRule:**
- `bool IsBroken()`
- `string Message { get; }`

**BusinessRuleValidationException:**
- Takes `IBusinessRule` in constructor
- Exposes `BrokenRule` and `Details`

**Acceptance Criteria:**
- [ ] No external dependencies except MediatR
- [ ] All base classes are abstract
- [ ] Value object equality works correctly
- [ ] Domain events have unique IDs

---

## Task 1.5: Create BuildingBlocks.Application

**Agent:** Module Implementer

**Description:**
Create shared application abstractions for CQRS, Result pattern, and pipeline behaviors.

**Files to create:**

```
src/BuildingBlocks/EnglishTutor.BuildingBlocks.Application/
├── EnglishTutor.BuildingBlocks.Application.csproj
├── Results/
│   ├── Result.cs
│   ├── Result{T}.cs
│   └── Error.cs
├── Pagination/
│   └── PagedResult{T}.cs
├── Abstractions/
│   ├── ICommand.cs
│   ├── ICommandHandler.cs
│   ├── IQuery.cs
│   ├── IQueryHandler.cs
│   ├── IDateTimeProvider.cs
│   └── ICurrentUser.cs
├── Behaviors/
│   ├── ValidationBehavior.cs
│   └── LoggingBehavior.cs
└── Exceptions/
    ├── NotFoundException.cs
    └── ValidationException.cs
```

**Result pattern:**
- `Result` — success/failure with `Error`
- `Result<T>` — success with value or failure with `Error`
- `Error` — record with `Code` and `Message`
- Static factory: `Result.Success()`, `Result.Failure(Error)`, `Result<T>.Success(value)`, `Result<T>.Failure(Error)`
- `IsSuccess`, `IsFailure` properties

**CQRS abstractions (via MediatR):**
- `ICommand : IRequest<Result>`
- `ICommand<TResponse> : IRequest<Result<TResponse>>`
- `ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>`
- `IQuery<TResponse> : IRequest<Result<TResponse>>`
- `IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>`

**ICurrentUser:**
- `Guid UserId { get; }`
- `bool IsAuthenticated { get; }`
- `string? Email { get; }`

**IDateTimeProvider:**
- `DateTime UtcNow { get; }`

**ValidationBehavior<TRequest, TResponse>:**
- `IPipelineBehavior` that runs `IValidator<TRequest>` validators before handler
- If validation fails, return `Result.Failure` with validation errors

**LoggingBehavior<TRequest, TResponse>:**
- Log command/query name, execution time
- Include correlation ID

**PagedResult<T>:**
- `List<T> Items`
- `int TotalCount`
- `int Page`
- `int PageSize`
- `int TotalPages`

**Acceptance Criteria:**
- [ ] References only `BuildingBlocks.Domain`, `MediatR`, `FluentValidation`
- [ ] Result pattern is immutable
- [ ] Pipeline behaviors registered in correct order (validation → logging → handler)
- [ ] PagedResult calculates TotalPages correctly

---

## Task 1.6: Create BuildingBlocks.Infrastructure

**Agent:** Module Implementer

**Description:**
Create shared infrastructure implementations.

**Files to create:**

```
src/BuildingBlocks/EnglishTutor.BuildingBlocks.Infrastructure/
├── EnglishTutor.BuildingBlocks.Infrastructure.csproj
├── DateTimeProvider.cs
├── CurrentUser.cs
├── Serialization/
│   └── JsonSerializerService.cs
├── Storage/
│   ├── IFileStorageService.cs
│   └── LocalFileStorageService.cs (dev only)
└── Persistence/
    └── DomainEventDispatcher.cs
```

**DateTimeProvider : IDateTimeProvider:**
- Returns `DateTime.UtcNow`

**CurrentUser : ICurrentUser:**
- Reads from `IHttpContextAccessor` → `ClaimsPrincipal`
- Extracts `UserId` from JWT `sub` claim
- Extracts `Email` from JWT `email` claim

**DomainEventDispatcher:**
- Takes `IMediator`
- Dispatches all domain events from aggregate root after SaveChanges
- Pattern: `SaveChangesInterceptor` or called explicitly before `SaveChangesAsync`

**Acceptance Criteria:**
- [ ] References `BuildingBlocks.Application`
- [ ] CurrentUser correctly parses JWT claims
- [ ] DomainEventDispatcher publishes all pending domain events

---

## Task 1.7: Create BuildingBlocks.EventBus

**Agent:** Integration & Events Agent

**Description:**
Create event bus abstractions and in-process implementation.

**Files to create:**

```
src/BuildingBlocks/EnglishTutor.BuildingBlocks.EventBus/
├── EnglishTutor.BuildingBlocks.EventBus.csproj
├── IIntegrationEvent.cs
├── IntegrationEvent.cs
├── IIntegrationEventHandler.cs
├── IEventBus.cs
└── InProcess/
    └── InProcessEventBus.cs
```

**IIntegrationEvent:**
- `Guid EventId { get; }`
- `DateTime OccurredOnUtc { get; }`
- `string EventType { get; }`

**IntegrationEvent (abstract record):**
- Auto-generates `EventId = Guid.NewGuid()`
- Sets `OccurredOnUtc = DateTime.UtcNow`
- `EventType` defaults to concrete class name

**IIntegrationEventHandler<TEvent>:**
- `Task HandleAsync(TEvent @event, CancellationToken ct)`

**IEventBus:**
- `Task PublishAsync(IIntegrationEvent @event, CancellationToken ct)`

**InProcessEventBus:**
- Resolves all `IIntegrationEventHandler<TEvent>` from DI
- Calls each handler sequentially
- Logs handler name + event type + duration

**Acceptance Criteria:**
- [ ] No external messaging dependencies (no RabbitMQ/Kafka)
- [ ] Handlers resolved from DI container
- [ ] Event type serializable to string for Outbox storage
- [ ] Abstraction allows future RabbitMQ adapter

---

## Task 1.8: Create BuildingBlocks.Outbox

**Agent:** Integration & Events Agent

**Description:**
Create Outbox/Inbox/Dead-letter abstractions for reliable event delivery.

**Files to create:**

```
src/BuildingBlocks/EnglishTutor.BuildingBlocks.Outbox/
├── EnglishTutor.BuildingBlocks.Outbox.csproj
├── OutboxMessage.cs
├── InboxMessage.cs
├── DeadLetterMessage.cs
├── OutboxMessageStatus.cs
├── IOutboxWriter.cs
├── IOutboxProcessor.cs
├── IInboxChecker.cs
├── OutboxOptions.cs
└── Processing/
    ├── OutboxBackgroundJob.cs
    └── RetryPolicy.cs
```

**OutboxMessage:**
```
Id (Guid)
EventId (Guid)
EventType (string)
Payload (string — JSON)
Status (Pending/Processing/Processed/Failed)
RetryCount (int)
MaxRetryCount (int, default 5)
NextRetryAtUtc (DateTime?)
LockedBy (string?)
LockedUntilUtc (DateTime?)
CreatedAtUtc (DateTime)
ProcessedAtUtc (DateTime?)
LastError (string?)
```

**InboxMessage:**
```
Id (Guid)
EventId (Guid)
EventType (string)
HandlerName (string)
ProcessedAtUtc (DateTime)
```
- Unique index on `(EventId, HandlerName)`

**DeadLetterMessage:**
```
Id (Guid)
EventId (Guid)
EventType (string)
Payload (string)
SourceModule (string)
FailedAtUtc (DateTime)
RetryCount (int)
LastError (string)
StackTrace (string?)
Status (Dead/Reprocessed)
```

**IOutboxWriter:**
- `void Add(OutboxMessage message)` — called within SaveChanges transaction

**IOutboxProcessor:**
- `Task ProcessPendingMessagesAsync(CancellationToken ct)`
- Fetch pending/failed messages with `LockedUntilUtc < UtcNow`
- Lock rows (set LockedBy, LockedUntilUtc)
- Deserialize event, publish via `IEventBus`
- Mark as Processed or increment RetryCount
- Move to DeadLetter if RetryCount > MaxRetryCount

**IInboxChecker:**
- `Task<bool> IsProcessedAsync(Guid eventId, string handlerName, CancellationToken ct)`
- `Task MarkAsProcessedAsync(Guid eventId, string handlerName, CancellationToken ct)`

**RetryPolicy:**
- Exponential backoff: `NextRetryAtUtc = UtcNow + 2^retryCount * 10 seconds`
- Max 5 retries

**OutboxBackgroundJob : IJob (Quartz):**
- Calls `IOutboxProcessor.ProcessPendingMessagesAsync()`
- Runs every 5 seconds (configurable via `OutboxOptions`)

**Acceptance Criteria:**
- [ ] OutboxMessage supports row-locking fields for horizontal Worker scaling
- [ ] InboxMessage prevents duplicate event handling per handler
- [ ] DeadLetterMessage captures full error context
- [ ] Retry policy uses exponential backoff
- [ ] OutboxBackgroundJob is Quartz-compatible

---

## Task 1.9: Create BuildingBlocks.SharedKernel

**Agent:** Module Implementer

**Description:**
Create tiny shared value objects used across modules.

**Files to create:**

```
src/BuildingBlocks/EnglishTutor.BuildingBlocks.SharedKernel/
├── EnglishTutor.BuildingBlocks.SharedKernel.csproj
├── LanguageCode.cs
├── LanguageLevel.cs
└── LearningSkill.cs
```

**LanguageCode (ValueObject):**
- `string Value` — ISO 639-1 (e.g., "en", "vi", "ja")
- Validation: not empty, 2-3 characters
- Static factories: `LanguageCode.English`, `LanguageCode.Vietnamese`

**LanguageLevel (enum):**
```
A1, A2, B1, B2, C1, C2
```

**LearningSkill (enum):**
```
Vocabulary, Grammar, Pronunciation, Speaking, Listening, Reading, Writing, Conversation
```

**Acceptance Criteria:**
- [ ] References only `BuildingBlocks.Domain`
- [ ] LanguageCode validates format
- [ ] No business logic — only shared value types
- [ ] Stays small (under 100 total lines)

---

## Task 1.10: Create Architecture Tests Project

**Agent:** Architecture Guardian

**Description:**
Create architecture test project with initial dependency rules.

**Files to create:**

```
tests/EnglishTutor.ArchitectureTests/
├── EnglishTutor.ArchitectureTests.csproj
├── ModuleBoundaryTests.cs
├── LayerDependencyTests.cs
└── TestConstants.cs
```

**TestConstants:**
- Define namespace constants for each layer and module
- `DomainNamespace = "EnglishTutor.Modules.*.Domain"`
- `ApplicationNamespace = "EnglishTutor.Modules.*.Application"`
- `InfrastructureNamespace = "EnglishTutor.Modules.*.Infrastructure"`
- `PresentationNamespace = "EnglishTutor.Modules.*.Presentation"`
- `ContractsNamespace = "EnglishTutor.Modules.*.Contracts"`

**LayerDependencyTests (initial rules):**
- Domain must not depend on Application
- Domain must not depend on Infrastructure
- Domain must not depend on Presentation
- Application must not depend on Infrastructure
- Application must not depend on Presentation

**ModuleBoundaryTests (initial rules):**
- Module A must not reference Module B's Domain
- Module A must not reference Module B's Application
- Module A must not reference Module B's Infrastructure
- Module A may only reference Module B's Contracts
- Contracts must not expose types from Domain namespace

**Acceptance Criteria:**
- [ ] Uses NetArchTest.Rules
- [ ] Tests pass with current empty module structure
- [ ] Tests will fail when architecture rules are violated
- [ ] At least 10 initial architecture rules defined

---

## Task 1.11: Global Exception Handling & Result Pattern Integration

**Agent:** Module Implementer

**Description:**
Wire the Result pattern into API response pipeline. This extends Task 1.2 middleware.

**Files to create/modify:**

```
src/Bootstrapper/EnglishTutor.Api/Extensions/
├── ResultExtensions.cs
└── EndpointFilterExtensions.cs
```

**ResultExtensions:**
- `IResult ToHttpResult(this Result result)` — maps Result to appropriate HTTP status
- `IResult ToHttpResult<T>(this Result<T> result)` — 200 with value or error status
- Success → 200/201
- Failure with `NotFound` code → 404
- Failure with `Validation` code → 422
- Failure with `Conflict` code → 409
- Failure with `Unauthorized` code → 401
- Failure with `Forbidden` code → 403

**Response envelope:**
```json
{
  "isSuccess": true,
  "data": { ... },
  "error": null
}
```
```json
{
  "isSuccess": false,
  "data": null,
  "error": { "code": "Validation", "message": "..." }
}
```

**Acceptance Criteria:**
- [ ] All endpoints return consistent response envelope
- [ ] Error codes map to correct HTTP status codes
- [ ] ProblemDetails used for unhandled exceptions
- [ ] Validation errors return field-level details

---

## Task 1.12: Create Empty Module Folder Structure

**Agent:** Module Implementer

**Description:**
Create placeholder `.csproj` projects for all 13 modules so the solution structure exists.

**Folders to create (each with 5 sub-projects):**

```
src/Modules/Auth/
  ├── EnglishTutor.Modules.Auth.Domain/
  ├── EnglishTutor.Modules.Auth.Application/
  ├── EnglishTutor.Modules.Auth.Infrastructure/
  ├── EnglishTutor.Modules.Auth.Presentation/
  └── EnglishTutor.Modules.Auth.Contracts/
```

Repeat for: `Users`, `StudyPlans`, `LearningContent`, `Vocabulary`, `Exercises`, `Speaking`, `AI`, `Mistakes`, `Assessments`, `Progress`, `Notifications`, `AdminReports`

**Total: 13 modules × 5 projects = 65 `.csproj` files**

**Each `.csproj` should:**
- Reference correct BuildingBlocks layer:
  - `*.Domain.csproj` → references `BuildingBlocks.Domain`, `BuildingBlocks.SharedKernel`
  - `*.Application.csproj` → references own `*.Domain`, `BuildingBlocks.Application`
  - `*.Infrastructure.csproj` → references own `*.Application`, own `*.Domain`, `BuildingBlocks.Infrastructure`, `BuildingBlocks.Outbox`
  - `*.Presentation.csproj` → references own `*.Application`, `BuildingBlocks.Application`
  - `*.Contracts.csproj` → references `BuildingBlocks.EventBus` (for IntegrationEvent base)
- Not specify `TargetFramework` or package versions (inherited from `Directory.Build.props`)

**Acceptance Criteria:**
- [ ] All 65 projects created and added to solution
- [ ] `dotnet build` passes
- [ ] Correct project references per layer
- [ ] No package version in any `.csproj`

---

## Task 1.13: Write Initial ADR Documents

**Agent:** Documentation Agent

**Description:**
Create Architecture Decision Records.

**Files to create:**

```
docs/adr/0001-modular-monolith.md
docs/adr/0002-module-owned-dbcontext.md
docs/adr/0003-cross-module-communication.md
docs/adr/0004-internal-messaging-outbox-inbox.md
```

**Each ADR follows format:**
```
# ADR-NNNN: Title
## Status: Accepted
## Context: Why this decision was needed
## Decision: What was decided
## Consequences: Trade-offs and implications
```

**ADR-0001:** Why modular monolith over microservices or traditional monolith
**ADR-0002:** Why each module owns its own DbContext and schema
**ADR-0003:** Contract Readers vs Integration Events vs Read Models — when to use each
**ADR-0004:** Why DB-backed Outbox/Inbox over RabbitMQ, retry/dead-letter strategy

**Acceptance Criteria:**
- [ ] 4 ADR documents created
- [ ] Each follows standard ADR format
- [ ] Context sections reference project-specific reasoning
- [ ] Consequences list trade-offs honestly

---

## Task 1.14: Docker Compose for Infrastructure

**Agent:** Module Implementer

**Description:**
Create Docker Compose file for local development infrastructure.

**Files to create:**

```
d:\Projects\EnglishTutor\docker-compose.yml
d:\Projects\EnglishTutor\docker-compose.override.yml
d:\Projects\EnglishTutor\.env.example
```

**Services:**

| Service | Image | Port | Schema |
|---------|-------|------|--------|
| postgres | postgres:17 | 5432 | all module schemas |
| redis | redis:7-alpine | 6379 | cache, rate limit |
| pgadmin | dpage/pgadmin4 | 5050 | tools profile |
| redis-insight | redis/redisinsight | 5540 | tools profile |

**Configuration:**
- PostgreSQL: `english_tutor_db`, user/password from `.env`
- Volume: `postgres_data` for persistence
- pgAdmin + Redis Insight under `profiles: ["tools"]`
- Health checks on postgres and redis

**Acceptance Criteria:**
- [ ] `docker compose up -d` starts postgres + redis
- [ ] `docker compose --profile tools up -d` starts management tools
- [ ] Data persists across restarts via volume
- [ ] `.env.example` documents all required variables

---

## Phase 1 Definition of Done

- [ ] `dotnet build` passes for entire solution (65+ projects)
- [ ] `dotnet test` passes (architecture tests)
- [ ] API host starts → Swagger accessible → health check returns 200
- [ ] Worker host starts → Quartz scheduler initializes
- [ ] Docker Compose starts PostgreSQL + Redis
- [ ] 4 ADR documents created
- [ ] Result pattern + exception handling + correlation ID wired
- [ ] Outbox/Inbox/DeadLetter abstractions ready
- [ ] EventBus abstraction ready with InProcess implementation
