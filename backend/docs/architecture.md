# EnglishLiveTutor backend — Pragmatic Clean Architecture

This is **Pragmatic Clean Architecture + DDD aggregates + feature-oriented CQRS**, not a modular monolith.

## Projects

- `src/EnglishLiveTutor.Api`: ASP.NET Core minimal API, SignalR hubs, startup, HTTP current-user adapter.
- `src/EnglishLiveTutor.Application`: CQRS handlers, DTOs and interfaces; references Domain.
- `src/EnglishLiveTutor.Domain`: domain types grouped by aggregate root; has no project dependencies.
- `src/EnglishLiveTutor.Infrastructure`: SQL Server EF Core, repositories, Gemini AI adapters, realtime services; references Domain/Application.
- `tests/EnglishLiveTutor.Domain.Tests`: domain unit tests.

API composes Application and Infrastructure. There is one AppDbContext. No BuildingBlocks project.

## Domain inheritance

- `Entity<TId> : IAuditable`, on all 22 persisted domain entity types.
- `AggregateRoot<TId> : Entity<TId>, ISoftDeletable`, on 10 independently persisted roots.
- Vocabulary child entities do **not** implement ISoftDeletable.
- `AiModelConfigEntity` and `VocabularyGenerationAuditEntity` are independent roots, **not** children of Vocabulary.

Existing timestamps **CreatedAt / UpdatedAt** are deliberately retained to prevent breaking API/SQL consumers; UTC read-only aliases `CreatedAtUtc` / `UpdatedAtUtc` are available but not mapped. New audit metadata is CreatedBy/UpdatedBy. For existing tables lacking CreatedAt/UpdatedAt, the additive migration adds them.

`IsActive` has different semantics than soft deletion and remains unchanged.

## Persistence and migration safeguards

- `AuditingSaveChangesInterceptor` sets user audit metadata (current HTTP principal or `system`) and update time; preserves explicit creation times from imported data.
- `DbContext.Remove(root)` is prevented; `ConversationRepository.DeleteConversationAsync` now calls `SoftDelete()`.
- EF query filters hide deleted roots. **Direct SQL, ExecuteDelete, and standalone queries on child tables do not inherit root filters automatically**; consider them carefully before publishing new endpoints.
- Migration `20261015000000_AddEntityAuditAndSoftDelete` adds only columns and one nullable alteration; it is not automatically applied by startup. Make a SQL backup and inspect a generated migration script before applying to a real database.
- Existing EF migrations are preserved verbatim. The model snapshot corrects previously stale CLR namespace identifiers for future migrations.
- The old `DatabaseSeeder` uses raw SQL `IF OBJECT_ID` bootstrap scripts. Its schema creation is not a replacement for migrations. Use EF migrations on an existing populated database first. It may require further consolidation after migration validation.

## Build and run

```
dotnet restore src/EnglishLiveTutor.Api/EnglishLiveTutor.Api.csproj
dotnet build src/EnglishLiveTutor.Api/EnglishLiveTutor.Api.csproj
dotnet test tests/EnglishLiveTutor.Domain.Tests/EnglishLiveTutor.Domain.Tests.csproj
# After backup and inspecting SQL:
dotnet ef migrations script --idempotent --project src/EnglishLiveTutor.Infrastructure --startup-project src/EnglishLiveTutor.Api --output update.sql
```

**Environment note:** The archive was prepared in a container without .NET SDK and without a configured SQL Server. `dotnet build`, `dotnet test`, and EF schema verification could not be executed; do not treat this ZIP as production-verified.

## Caveats and follow-up

1. Current Domain objects still expose several public mutable business properties/collections. Encapsulating all aggregate invariants is a separate behavioral refactor requiring integration tests and call-site updates.
2. The Application write-side currently accesses DbSet through IAppDbContext in some features; migrating all use cases to aggregate repositories was intentionally not forced here to minimize risk.
3. Existing endpoint contracts and AI adapters were carried over without deliberate contract changes. Please run API and websocket integration tests.
4. The original uploaded ZIP should be retained as a rollback artifact. Legacy root-level code was excluded from compilation and removed from this refactored source ZIP.


## Identity + prompts + Google teacher voices (2026-10-15)

- `Domain/Identity`: User, Role and Permission aggregates, UserRole/RolePermission/RefreshToken entities.
- `Domain/Prompts`: editable `AiPromptTemplateEntity` aggregate; `Infrastructure/Persistence/CefrPromptSeedCatalog` inserts the original CEFR/mode directives.
- `Domain/TeacherVoices`: `TeacherVoiceEntity` aggregate; 30 prebuilt Google Gemini Live voices seeded with 6 Vietnamese recommended options.
- `Infrastructure/Identity/IdentityService`: password hashing, JWT refresh token rotation/revocation, role/permission management.
- `Api/Endpoints/IdentityEndpoints`: `/api/auth` and `/api/admin`; `AiConfigurationEndpoints`: `/api/ai-prompts`, `/api/teacher-voices`.
- Migration `20261015010000_AddIdentityPromptsTeacherVoices`: 8 additive tables.

See `README_IDENTITY_PROMPTS_TEACHER_VOICES.md` for full setup and security boundaries.
