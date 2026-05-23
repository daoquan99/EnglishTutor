# Database rules

## Topology

- Single physical Postgres database `english_tutor_db`.
- One schema per module, plus `messaging` for outbox/inbox.
- One `DbContext` per module, scoped to its schema. One module never accesses another module's `DbContext`.

## Schemas

| Schema           | Module / system                            | Migration assembly                                    |
| ---------------- | ------------------------------------------ | ----------------------------------------------------- |
| `auth`           | Identity, permissions                      | `EnglishTutor.Modules.Auth.Infrastructure`            |
| `users`          | Profile, language settings                 | `EnglishTutor.Modules.Users.Infrastructure`           |
| `studyplans`     | Plans, calendar, targets                   | `EnglishTutor.Modules.StudyPlans.Infrastructure`      |
| `learningcontent`| Curriculum, lessons, conversations         | `EnglishTutor.Modules.LearningContent.Infrastructure` |
| `vocabulary`     | Flashcards, mastery, examples              | `EnglishTutor.Modules.Vocabulary.Infrastructure`      |
| `exercises`      | Exercise engine, attempts                  | `EnglishTutor.Modules.Exercises.Infrastructure`       |
| `speaking`       | Speaking sessions, transcripts             | `EnglishTutor.Modules.Speaking.Infrastructure`        |
| `ai`             | AI logs, costs, quotas                     | `EnglishTutor.Modules.AI.Infrastructure`              |
| `mistakes`       | Incorrect-answer aggregator                | `EnglishTutor.Modules.Mistakes.Infrastructure`        |
| `assessments`    | Placement / level-up exams                 | `EnglishTutor.Modules.Assessments.Infrastructure`     |
| `progress`       | EXP, streak, dashboards                    | `EnglishTutor.Modules.Progress.Infrastructure`        |
| `notifications`  | Queued outbound notifications              | `EnglishTutor.Modules.Notifications.Infrastructure`   |
| `adminreports`   | Reporting projections                      | `EnglishTutor.Modules.AdminReports.Infrastructure`    |
| `messaging`      | Outbox, Inbox, DLQ                         | `EnglishTutor.Worker` (`MessagingDbContext`)          |

## EF Core rules

- `IEntityTypeConfiguration<T>` files are for mapping only. No business logic.
- One config file per aggregate root (and grouped child entities) under `Infrastructure/Persistence/Configurations/`.
- Migrations live at `Infrastructure/Persistence/Migrations/` — pass `--output-dir Persistence/Migrations` when generating.
- Use `ToTable("name", "schema")` to pin tables to the module schema.
- Configure soft-delete query filters in `OnModelCreating` for every `ISoftDeletable` aggregate (or use the project's central filter helper).
- Audit fields are populated by `AuditingSaveChangesInterceptor` (or equivalent). Do not set them manually.

## Foreign keys

- Cross-module references use IDs (Guid value objects or strongly-typed IDs) only. No navigation properties across modules. No cross-module FK constraints.
- Within a single module, normal FK relationships and navigations are fine.

## Migration commands

From `EnglishTutor.API/`:

```bash
# add a migration for one module
dotnet ef migrations add <Name> \
  --project src/Modules/Vocabulary/EnglishTutor.Modules.Vocabulary.Infrastructure \
  --startup-project src/Bootstrapper/EnglishTutor.Api \
  --context VocabularyDbContext \
  --output-dir Persistence/Migrations

# apply migrations (or set Database__AutoMigrate=true on startup)
dotnet ef database update \
  --project src/Modules/Vocabulary/EnglishTutor.Modules.Vocabulary.Infrastructure \
  --startup-project src/Bootstrapper/EnglishTutor.Api \
  --context VocabularyDbContext

# Worker messaging migrations
dotnet ef migrations add <Name> \
  --project src/Bootstrapper/EnglishTutor.Worker \
  --startup-project src/Bootstrapper/EnglishTutor.Worker \
  --context MessagingDbContext \
  --output-dir Outbox/Migrations
```

## Resetting local DB

```bash
./scripts/Reset-DevDatabase.ps1
```

Drops the Docker volume, restarts Postgres+Redis. Then start API/Worker with `Database__AutoMigrate=true` to recreate schemas.

## Forbidden

- Cross-module joins or FK constraints.
- Direct read of another module's tables.
- Cross-module database views for core business flows (analytics in `adminreports` is the exception).
- Adding tables/columns without a migration.
- Mutating an already-applied migration.
