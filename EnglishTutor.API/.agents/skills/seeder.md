---
name: seeder
description: Adds seed data for a module — initial reference data, dev fixtures, or test data loaded on startup. Follows the IModuleSeeder pattern with deterministic ordering.
---

# Seeder

## When to invoke

- A new module needs initial reference data (e.g., default permissions, language lists, exercise templates).
- An existing module gains a new entity that requires seed data.
- Adding dev-only test fixtures for local development.

## Architecture

```
{Module}.Infrastructure/
└── Seed/
    ├── {Module}ModuleSeeder.cs      # IModuleSeeder entry point
    ├── {Module}DataSeeder.cs        # Actual seeding logic
    └── {Module}SeedData.cs          # Static data definitions (optional)
```

## Step-by-step

### 1. Create (or extend) the data seeder

`src/Modules/{Module}/EnglishTutor.Modules.{Module}.Infrastructure/Seed/{Module}DataSeeder.cs`:

```csharp
using EnglishTutor.Modules.{Module}.Infrastructure.Persistence;

namespace EnglishTutor.Modules.{Module}.Infrastructure.Seed;

public sealed class {Module}DataSeeder({Module}DbContext dbContext)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.{Entities}.AnyAsync(cancellationToken))
            return;

        var items = {Module}SeedData.GetDefaultItems();
        dbContext.{Entities}.AddRange(items);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

Rules:
- Idempotent — check if data already exists before inserting.
- Use deterministic IDs (hardcoded Guids) for reference data so migrations and tests can rely on them.
- Use `IDateTimeProvider` or a fixed UTC date for timestamps — never `DateTime.Now`.

### 2. Create the module seeder

`src/Modules/{Module}/EnglishTutor.Modules.{Module}.Infrastructure/Seed/{Module}ModuleSeeder.cs`:

```csharp
using EnglishTutor.BuildingBlocks.Infrastructure.Seeding;

namespace EnglishTutor.Modules.{Module}.Infrastructure.Seed;

public sealed class {Module}ModuleSeeder({Module}DataSeeder dataSeeder) : IModuleSeeder
{
    public int Order => {N};          // determines execution order across modules
    public string ModuleName => "{Module}";

    public Task SeedAsync(CancellationToken cancellationToken = default) =>
        dataSeeder.SeedAsync(cancellationToken);
}
```

### 3. Register in DI

`src/Modules/{Module}/EnglishTutor.Modules.{Module}.Infrastructure/DependencyInjection.cs`:

```csharp
services.AddScoped<{Module}DataSeeder>();
services.AddScoped<IModuleSeeder, {Module}ModuleSeeder>();
```

### 4. Determine Order value

Seeders run in ascending `Order`. Guidelines:

| Range   | Purpose                                       |
| ------- | --------------------------------------------- |
| 0–9     | Auth (roles, permissions, admin user)         |
| 10–19   | Users, LearningContent (reference data)       |
| 20–29   | StudyPlans, Exercises, Assessments            |
| 30–39   | Vocabulary, Speaking, Mistakes                |
| 40–49   | Progress, Notifications                       |
| 50+     | AdminReports, cross-module test fixtures      |

Pick a value that ensures dependencies are seeded first (e.g., Vocabulary seeds after LearningContent because items reference content topics).

### 5. Static seed data (optional helper)

`src/Modules/{Module}/EnglishTutor.Modules.{Module}.Infrastructure/Seed/{Module}SeedData.cs`:

```csharp
namespace EnglishTutor.Modules.{Module}.Infrastructure.Seed;

internal static class {Module}SeedData
{
    public static IReadOnlyList<{Entity}> GetDefaultItems() =>
    [
        // use deterministic Guids for referenceability
        {Entity}.Create(new Guid("..."), ...),
    ];
}
```

## Forbidden

- Using `DateTime.Now` or non-UTC timestamps in seed data.
- Non-idempotent seeders (must check existence before insert).
- Seeding from external APIs or files that may change between runs.
- Cross-module seeding (module A's seeder must not write to module B's DbContext).
- Random Guids for reference data that other modules depend on.

## Commands

```bash
cd EnglishTutor.API
dotnet build EnglishTutor.slnx
# Run the API with seeding enabled:
dotnet run --project src/Bootstrapper/EnglishTutor.Api
```

Seeding runs automatically on startup when `Database__AutoMigrate=true` or `Database__SeedOnStartup=true`.

## Done when

- Seeder is idempotent (safe to run repeatedly).
- Order value respects inter-module dependencies.
- DI registered.
- `dotnet build` passes.
- API starts and seed data appears in the correct schema.
