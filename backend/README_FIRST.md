# EnglishLiveTutor — refactored backend (Pragmatic Clean Architecture)

**This is a refactored source archive, not a production-verified release.**

## Included changes

- API relocated to `src/EnglishLiveTutor.Api`; other three clean-architecture projects stay under `src`.
- Domain now has `IAuditable`, `ISoftDeletable`, `Entity<TId>`, `AggregateRoot<TId>`.
- All 22 EF-persisted domain entities use the base classes; 10 aggregate roots support logical deletion.
- Vocabulary entities, Conversations, Learning and Pronunciation domain classes split into files grouped by aggregate.
- 18 CQRS use cases organized into `Commands/<UseCase>` or `Queries/<UseCase>` folders.
- New EF audit interceptor, current-user abstraction, root soft-delete query filters.
- Conversation deletion converted from hard delete to soft delete.
- Existing EF migration files retained, plus additive `20261015000000_AddEntityAuditAndSoftDelete` and synchronized snapshot.
- Original excluded legacy root-level code removed from the refactored ZIP. Keep the uploaded original ZIP as backup.

## Verify on a machine with .NET 10 SDK

From `backend`:

```powershell
dotnet restore src/EnglishLiveTutor.Api/EnglishLiveTutor.Api.csproj
dotnet build src/EnglishLiveTutor.Api/EnglishLiveTutor.Api.csproj
dotnet test tests/EnglishLiveTutor.Domain.Tests/EnglishLiveTutor.Domain.Tests.csproj
```

**Before applying the database migration**, back up your SQL Server database, then review the migration script:

```powershell
dotnet ef migrations script --idempotent --project src/EnglishLiveTutor.Infrastructure --startup-project src/EnglishLiveTutor.Api --output review-migration.sql
```

Do not apply the migration without reviewing actual schema differences on your database. The original seed/bootstrap logic includes hand-written SQL, and a SQL Server connection was unavailable during this transformation. The model snapshot originally referenced pre-refactor entity namespace strings, now reconciled.

## Limitations

- This environment has no `dotnet` executable or SQL Server, so **build, tests and migrations have not been executed**.
- The API/SignalR and AI use cases require runtime smoke testing.
- Existing aggregate child collections remain mutable for compatibility; further encapsulation and repository-only write access should be a separate behavioral refactor after the project is compiled and end-to-end tested.
- Existing source preserved old public timestamp property names `CreatedAt`/`UpdatedAt`, mapped to SQL; `IAuditable` presents UTC getters `CreatedAtUtc`/`UpdatedAtUtc`. New columns are additive.

See `docs/architecture.md` for details.
