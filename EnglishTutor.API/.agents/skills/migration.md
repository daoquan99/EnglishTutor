---
name: migration
description: Adds EF Core migrations for module schemas or the messaging schema. Covers when to generate, naming, output directory, startup project selection, and verification.
---

# Migration

## When to invoke

- New entity / aggregate root added to a module's DbContext.
- Column added, renamed, or type-changed on an existing entity.
- Index, FK, or unique constraint added/changed.
- Outbox/Inbox schema change in the messaging layer.
- Soft-delete filter or audit column change.

## Inputs

- Module name (e.g., `Vocabulary`) or `Messaging` for outbox/inbox.
- Short description of the schema change (used in migration name).

## Step-by-step

### 1. Confirm the DbContext change compiles

```bash
cd EnglishTutor.API
dotnet build EnglishTutor.slnx
```

### 2. Generate the migration

**Module migration:**

```bash
cd EnglishTutor.API
dotnet ef migrations add <MigrationName> \
  --project src/Modules/{Module}/EnglishTutor.Modules.{Module}.Infrastructure \
  --startup-project src/Bootstrapper/EnglishTutor.Api \
  --context {Module}DbContext \
  --output-dir Persistence/Migrations
```

**Messaging migration (Outbox/Inbox/DLQ):**

```bash
cd EnglishTutor.API
dotnet ef migrations add <MigrationName> \
  --project src/Bootstrapper/EnglishTutor.Worker \
  --startup-project src/Bootstrapper/EnglishTutor.Worker \
  --context MessagingDbContext \
  --output-dir Outbox/Migrations
```

### 3. Name the migration meaningfully

- PascalCase, no underscores.
- Describe the change, not the entity: `AddPronunciationScoreColumn`, `CreateExampleFillBlankTable`, `AddVocabularyLearningUpgrade`.
- Don't use `Initial` unless it's the very first migration for the module.

### 4. Review the generated migration

- Open the `Up()` and `Down()` methods.
- Verify schema name matches: `schema: "{module_schema}"`.
- Verify no accidental table drops or column renames from EF model diff confusion.
- Verify `Down()` is the inverse of `Up()`.

### 5. Apply locally

Either:
- Set `Database__AutoMigrate=true` and restart the API, or:

```bash
dotnet ef database update \
  --project src/Modules/{Module}/EnglishTutor.Modules.{Module}.Infrastructure \
  --startup-project src/Bootstrapper/EnglishTutor.Api \
  --context {Module}DbContext
```

### 6. Verify

- Start the API and confirm the endpoint(s) still work.
- Run relevant unit/integration tests.

## Module → DbContext → Schema mapping

| Module          | DbContext                  | Schema           |
| --------------- | -------------------------- | ---------------- |
| Auth            | AuthDbContext              | auth             |
| Users           | UsersDbContext             | users            |
| StudyPlans      | StudyPlansDbContext        | studyplans       |
| LearningContent | LearningContentDbContext   | learningcontent  |
| Vocabulary      | VocabularyDbContext        | vocabulary       |
| Exercises       | ExercisesDbContext         | exercises        |
| Speaking        | SpeakingDbContext          | speaking         |
| AI              | AIDbContext                | ai               |
| Mistakes        | MistakesDbContext          | mistakes         |
| Assessments     | AssessmentsDbContext       | assessments      |
| Progress        | ProgressDbContext          | progress         |
| Notifications   | NotificationsDbContext     | notifications    |
| AdminReports    | AdminReportsDbContext      | adminreports     |
| (Worker)        | MessagingDbContext         | messaging        |

## Resetting local DB

```bash
./scripts/Reset-DevDatabase.ps1
```

Drops the Docker volume, restarts Postgres+Redis. Restart API/Worker with `Database__AutoMigrate=true`.

## Forbidden

- Mutating an already-applied migration (create a new one instead).
- Putting migration output anywhere other than `Persistence/Migrations` (modules) or `Outbox/Migrations` (messaging).
- Cross-module FK constraints in migration SQL.
- Manual `DateTime.Now` defaults — use `IDateTimeProvider`.
- Deleting the `__EFMigrationsHistory` table entry without dropping/recreating.

## Done when

- Migration compiles and `dotnet build` passes.
- `Up()` and `Down()` are correct inverses.
- Migration lands in the correct output directory.
- Schema name in migration matches the module.
- Local DB updated and API starts cleanly.
- `dotnet test tests/EnglishTutor.ArchitectureTests` passes (no new structural violations).
