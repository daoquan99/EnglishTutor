# Project Rules

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
- Use async/await for IO.
- Use CancellationToken in application handlers and infrastructure calls.
- Use Result pattern consistently with existing project conventions.

## Application Layer Rules

- Expose Commands and Queries via MediatR.
- Keep handlers focused — no god services.
- Do not create generic repositories unless the project already uses them.
- Avoid anemic command handlers that push all logic into infrastructure.
- Do not add unnecessary abstractions.
- Use `Result` / `Result<T>` for expected use-case outcomes.
- Keep module-specific error catalogs such as `AuthErrors` in the Application layer.
- Application error catalogs must use `BuildingBlocks.Application.Results.Error`.

## Domain Rules

- Domain logic stays inside Domain or Application layer.
- Do NOT put business logic in Controllers, Minimal API endpoints, EF Core configurations, or DI files.
- EF Core configurations are for mapping only.
- Domain entities raise Domain Events, NOT Integration Events.
- Application layer maps Domain Events to Integration Events when needed.
- Domain must NOT reference `BuildingBlocks.Application`.
- Domain must NOT use Application `Result`, `Result<T>`, or `Error`.
- Domain must NOT define module response error records just to carry `Code` and `Message`.
- Use `DomainException` / `BusinessRuleValidationException` for invalid domain invariants.
- Use `IBusinessRule` for named domain rules; throw `BusinessRuleValidationException` when a rule is broken.
- Do not use domain exceptions for normal expected use-case failures such as not found or invalid credentials; return Application `Result/Error` instead.

## Database Rules

- Each module has its own DbContext and schema.
- Migration history is schema-specific.
- No cross-module FK navigation properties.
- Cross-module references use IDs or value objects only.
- Within the same module, normal FK relationships are allowed.
- Do not query another module's tables directly.

## Integration Events Rules

- Producer modules save events into their own Outbox.
- Consumer modules use Inbox for idempotency.
- Event delivery is at-least-once.
- Event handlers must be idempotent.
- Event names must be past tense (e.g., `SpeakingSessionCompletedIntegrationEvent`).
- Do not publish events directly from request memory without Outbox.
- Use DeadLetterMessages for events that exceed retry limits.

## API Host Rules

- HTTP APIs, authentication/authorization, request validation.
- Execute commands/queries, save business data, save OutboxMessages.
- Stay stateless for horizontal scaling.
- Do NOT put long-running background jobs inside request handlers.

## Worker Host Rules

- Outbox processing, event dispatch, notifications, scheduled jobs.
- Projection rebuild jobs, weekly/monthly reports, async AI jobs.

## Centralized Build & Package Rules

- `Directory.Build.props` is the single place for common build settings.
- `Directory.Packages.props` is the single place for NuGet package versions.
- Do NOT specify package versions in individual `.csproj` files.
- Do NOT duplicate TargetFramework/Nullable/ImplicitUsings in module projects.
- Use stable, modern packages only. No preview/beta/rc/nightly unless approved.

## Documentation Rules

- Do NOT add or update an API without updating related documentation.
- Do NOT change request/response contracts without updating API docs.
- Do NOT add new integration events without updating event docs.
- Do NOT change a business workflow without updating workflow docs.
- Documentation updates are part of the Definition of Done.

### API docs must include:
```
Endpoint, Purpose, Auth requirement, Request body, Response body,
Validation rules, Error codes, Application flow, Related modules,
Integration events produced/consumed, Read models updated
```

### Workflow docs must include:
```
Overview, Main flow, Detailed steps, Modules involved,
Contracts used, Events published/consumed, Read models updated,
Failure/retry behavior
```

### Event docs must include:
```
Event name, Producer module, Consumer modules, Payload fields,
When published, Side effects, Idempotency notes, Related workflows
```

## Testing Rules

- Add unit tests for domain logic.
- Add handler tests for application logic where useful.
- Add integration tests for database queries and important flows.
- Add architecture tests for dependency rules.
- Do not skip failing tests.
- Before completing a task, run: `dotnet build` and `dotnet test`.

## Definition of Done

A task is NOT complete unless:
- Code compiles.
- Tests pass.
- New/updated APIs are documented.
- New/updated workflows are documented.
- New/updated integration events are documented.
- Architecture rules are not violated.
- No cross-module DbContext access is introduced.
- No IQueryable exposed across modules.
- No AI provider called outside AI.Infrastructure.

## Forbidden

- Do NOT rewrite the whole project.
- Do NOT change architecture without asking.
- Do NOT introduce direct module-to-module database joins.
- Do NOT expose IQueryable across modules.
- Do NOT access another module's DbContext.
- Do NOT reference another module's Infrastructure.
- Do NOT put all code into SharedKernel.
- Do NOT move Application Error/Result concepts into Domain or SharedKernel.
- Do NOT call external AI providers outside AI.Infrastructure.
- Do NOT add RabbitMQ/Kafka unless explicitly requested.
- Do NOT add EventHandlerExecutions unless explicitly requested.
- Do NOT introduce multi-tenancy for language.
- Do NOT add preview/beta/rc/nightly NuGet packages unless requested.

## Task Completion Summary

When finishing a task, provide:
```
- Changed files
- API changes
- Contract changes
- Event changes
- Read model/projection changes
- Docs updated
- Tests run
- Known limitations or follow-up tasks
```
