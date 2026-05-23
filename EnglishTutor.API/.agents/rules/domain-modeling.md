# Domain modeling rules

## Aggregate root vs entity

- Aggregate roots inherit `AggregateRoot<TId>` — owns domain events, implements `ISoftDeletable`.
- Non-root domain entities inherit `Entity<TId>` — implements `IAuditableEntity`.
- Repositories exist for aggregate roots only.
- Child entities are mutated through their aggregate root.
- If a child needs an independent lifecycle (own repo, own commands), promote it to an aggregate root and give it its own aggregate folder.

## Aggregate folder layout

Domain code is organized **per aggregate root**:

```
{Module}.Domain/
└── {AggregateRoot}/
    ├── {AggregateRoot}.cs            # AggregateRoot<TId>
    ├── Entities/                     # child Entity<TId>
    ├── ValueObjects/                 # immutable VOs (LanguageCode, MasteryScore, ...)
    ├── Enums/                        # business enums
    ├── Rules/                        # IBusinessRule implementations
    ├── Events/                       # DomainEvents raised by this aggregate
    └── Services/                     # pure domain services (no infra deps)
```

Aggregate-specific value objects/enums/rules/events stay near the aggregate that raises/uses them. When refactoring, move them with the aggregate, not into a global `ValueObjects/` bucket.

## Domain services

Domain services are for pure domain logic that doesn't naturally belong to an entity, aggregate, or value object.

Forbidden inside a domain service:
- Repository / `DbContext` access
- Cache / HTTP / external API calls
- AI providers
- `ICurrentUser`
- Any `Infrastructure` service

If you need those, use an Application service instead.

## Domain events

- Raised by the aggregate via `AddDomainEvent(...)`.
- Live in `Domain/{AggregateRoot}/Events/`.
- Past tense (`VocabularyMastered`, `SpeakingSessionCompleted`).
- Internal to the module — never reach another module directly. Application maps them to Integration Events when needed (`events.md`).

## Soft delete

- `ISoftDeletable` carries `IsDeleted` + `DeletedAtUtc` + `DeletedByUserId`.
- Implemented on `AggregateRoot<TId>` by default. A child entity implements `ISoftDeletable` only when it has a real independent soft-delete lifecycle.
- Soft delete is enforced via EF query filters configured in each `DbContext.OnModelCreating`.
- Do **not** add `IsActive` flags to base types. Model business state with aggregate-specific status enums (e.g., `SessionStatus.Pending|Active|Completed`).

## Audit fields

- `IAuditableEntity` carries `CreatedAtUtc`, `CreatedByUserId`, `UpdatedAtUtc`, `UpdatedByUserId`.
- Populated by `SaveChanges` interception. **Do not set audit fields manually** in handlers.
- Domain methods may set business timestamps that are part of the aggregate's behavior (e.g., `CompletedAtUtc` on a session) — those are not audit fields.

## UTC + Utc suffix

All persisted timestamps, API DTOs, integration events, domain events, and read models expose UTC with `Utc` suffix:

`CreatedAtUtc`, `UpdatedAtUtc`, `DeletedAtUtc`, `ExpiresAtUtc`, `OccurredOnUtc`, `StartedAtUtc`, `CompletedAtUtc`, `NextReviewAtUtc`, ...

Forbidden: `DateTime.Now`, local server time, timestamp properties without `Utc` suffix.

For user calendar-day boundaries (daily streak, weekly summary, ...): Application computes the boundary from the user's timezone setting and persists the resulting business date. The underlying timestamp stays UTC.

## Business rules

- Express named invariants with `IBusinessRule` (carries `IsBroken()` + `Message` + stable code).
- Aggregate code: `CheckRule(new SomeBusinessRule(...));` throws `BusinessRuleValidationException` when broken.
- Use `DomainException` for ad-hoc invariant violations that don't deserve a named rule.
- Domain exceptions are guardrails for invalid entity state — not use-case control flow. Use-case failures live in Application `Result` (see `error-handling.md`).

## Forbidden

- Adding navigation properties or FK references that cross module boundaries.
- Domain referencing `BuildingBlocks.Application` or any Application/Infrastructure type.
- Domain using `Result`/`Result<T>`/`Error` from Application.
- Anemic aggregates with all logic in the handler.
- Setting audit fields manually.
