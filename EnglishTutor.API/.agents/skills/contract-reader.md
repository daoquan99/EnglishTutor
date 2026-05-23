---
name: contract-reader
description: Exposes read-only data from one module to another via a Contract Reader interface in the Contracts project. The only legal way for module B to query module A's data synchronously.
---

# Contract Reader

## When to invoke

- Module B needs to read data owned by Module A without going through an integration event.
- A query handler needs data from another bounded context to enrich a response.
- A command handler needs to validate against another module's state (e.g., "does this vocabulary item exist?").

## Cross-module communication options

| Form              | Use when                                                       |
| ----------------- | -------------------------------------------------------------- |
| Contract Reader   | Synchronous read of another module's data (query enrichment)   |
| Integration Event | Async notification after a state change (eventual consistency) |
| Read Model        | Local projection updated via integration events                |

This skill covers Contract Readers only.

## Step-by-step

### 1. Define the reader interface in the provider module's Contracts

`src/Modules/{ProviderModule}/EnglishTutor.Modules.{ProviderModule}.Contracts/Readers/I{ProviderModule}Reader.cs`:

```csharp
namespace EnglishTutor.Modules.{ProviderModule}.Contracts.Readers;

public interface I{ProviderModule}Reader
{
    Task<VocabularyItemDto?> GetItemByIdAsync(Guid itemId, CancellationToken ct = default);
    Task<IReadOnlyList<VocabularyItemDto>> GetItemsByUserAsync(Guid userId, string targetLanguageCode, CancellationToken ct = default);
}
```

### 2. Define the DTO(s) in Contracts

`src/Modules/{ProviderModule}/EnglishTutor.Modules.{ProviderModule}.Contracts/DTOs/{Name}Dto.cs`:

```csharp
namespace EnglishTutor.Modules.{ProviderModule}.Contracts.DTOs;

public sealed record VocabularyItemDto(
    Guid Id,
    string Word,
    string TargetLanguageCode,
    string? PhoneticTranscription);
```

Rules:
- Primitives and value types only. No EF entities. No `IQueryable`.
- Include the `Utc` suffix on any timestamp fields.
- Keep DTOs focused — expose only what consumers actually need.

### 3. Implement the reader in the provider module's Infrastructure

`src/Modules/{ProviderModule}/EnglishTutor.Modules.{ProviderModule}.Infrastructure/Persistence/Readers/{ProviderModule}Reader.cs`:

```csharp
using EnglishTutor.Modules.{ProviderModule}.Contracts.DTOs;
using EnglishTutor.Modules.{ProviderModule}.Contracts.Readers;
using EnglishTutor.Modules.{ProviderModule}.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.{ProviderModule}.Infrastructure.Persistence.Readers;

internal sealed class {ProviderModule}Reader({ProviderModule}DbContext db) : I{ProviderModule}Reader
{
    public async Task<VocabularyItemDto?> GetItemByIdAsync(Guid itemId, CancellationToken ct = default)
    {
        return await db.VocabularyItems
            .Where(x => x.Id == itemId && !x.IsDeleted)
            .Select(x => new VocabularyItemDto(x.Id, x.Word, x.TargetLanguageCode.Value, x.PhoneticTranscription))
            .FirstOrDefaultAsync(ct);
    }
}
```

Rules:
- Use `.AsNoTracking()` or projection (`.Select(...)`) for reads.
- Filter soft-deleted records.
- Return DTOs, never entities.

### 4. Register in DI

`src/Modules/{ProviderModule}/EnglishTutor.Modules.{ProviderModule}.Infrastructure/DependencyInjection.cs`:

```csharp
services.AddScoped<I{ProviderModule}Reader, {ProviderModule}Reader>();
```

### 5. Consume in the other module

The consuming module references `EnglishTutor.Modules.{ProviderModule}.Contracts` (not Infrastructure or Domain).

```csharp
// In ConsumerModule.Application handler:
public sealed class SomeQueryHandler(I{ProviderModule}Reader vocabReader)
    : IQueryHandler<SomeQuery, SomeResponse>
{
    public async Task<Result<SomeResponse>> Handle(SomeQuery request, CancellationToken ct)
    {
        var item = await vocabReader.GetItemByIdAsync(request.ItemId, ct);
        if (item is null)
            return Result.Failure<SomeResponse>(ConsumerErrors.ReferencedItemNotFound);
        // ...
    }
}
```

### 6. Add project reference

In the consuming module's `.Application.csproj`:

```xml
<ProjectReference Include="..\..\{ProviderModule}\EnglishTutor.Modules.{ProviderModule}.Contracts\EnglishTutor.Modules.{ProviderModule}.Contracts.csproj" />
```

The Infrastructure project of the consuming module does **not** reference the provider's Infrastructure.

## Forbidden

- Injecting another module's `DbContext` directly.
- Returning `IQueryable` from a Contract Reader.
- Returning EF entities or Domain entities from a Contract Reader.
- Adding navigation properties across module boundaries.
- Adding FK constraints across module schemas.
- Making the reader interface `internal` — it must be `public` in Contracts.

## Commands

```bash
cd EnglishTutor.API
dotnet build EnglishTutor.slnx
dotnet test tests/EnglishTutor.ArchitectureTests
```

## Done when

- Interface in Contracts, implementation in Infrastructure, DI registered.
- Consumer references only Contracts project.
- DTO uses primitives + Utc-suffix timestamps.
- No `IQueryable` leak, no entity exposure.
- Architecture tests pass.
- Unit test for the consuming handler covers the reader-null path.
