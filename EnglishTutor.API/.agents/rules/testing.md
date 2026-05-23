# Testing rules

## Test projects

```
tests/
├── EnglishTutor.ArchitectureTests/                    # NetArchTest — module boundaries, layering
├── EnglishTutor.IntegrationTests/                     # cross-module DB / outbox-inbox flows
└── EnglishTutor.Modules.{Module}.UnitTests/           # one per module
```

## What goes where

- **Domain unit tests** — pure tests of aggregate behavior, invariants, value object equality, business rules. No mocks, no DB. Live in `Modules.{Module}.UnitTests`.
- **Handler tests** — command/query handlers tested with fake repositories and fake `IDateTimeProvider`/`ICurrentUser`. Live in `Modules.{Module}.UnitTests`.
- **Integration tests** — real Postgres (test schemas), real EF migrations, real outbox/inbox. Live in `EnglishTutor.IntegrationTests`. Use these for: repository queries, DbContext config, cross-module integration events, projection updates.
- **Architecture tests** — every layering / boundary rule from `architecture.md`. Live in `EnglishTutor.ArchitectureTests`.

## Running

From `EnglishTutor.API/`:

```bash
dotnet test EnglishTutor.slnx                                # all
dotnet test tests/EnglishTutor.Modules.Vocabulary.UnitTests  # single project
dotnet test tests/EnglishTutor.ArchitectureTests             # architecture only
dotnet test --filter "FullyQualifiedName~MarkMasteredCommandHandlerTests.Should_Succeed_When_Valid"
```

`scripts/Test.ps1` runs the full suite with DB cleanup. Pass `-SkipDatabaseCleanup` to skip the cleanup step.

## Conventions

- xUnit `[Fact]` / `[Theory]`. Use `Assert` (or `FluentAssertions` if already in the project).
- Test method names follow `Should_<Outcome>_When_<Condition>`.
- One arrange/act/assert per test. No shared mutable state.
- Don't mock the database for tests covering EF query shape or migration correctness — use the integration test project with a real Postgres.
- Don't skip failing tests. If a test cannot be run, explain why in the PR.

## Definition of done

- `dotnet build EnglishTutor.slnx` succeeds.
- `dotnet test EnglishTutor.slnx` passes (or each failure is explained).
- Architecture tests pass after any cross-module/layering change.
