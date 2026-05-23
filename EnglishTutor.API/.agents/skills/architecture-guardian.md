---
name: architecture-guardian
description: Validates module boundaries, layering, and architecture-test coverage after structural changes. Run after any cross-module change, new project reference, or contract addition.
---

# Architecture Guardian

## When to invoke

After any of:

- New module added.
- New project reference added.
- Cross-module change (new Contract Reader, new Integration Event, new dependency).
- Layering refactor.
- New AI usage path (might break AI-isolation rule).
- New outbox/inbox plumbing.

## What to check

1. **Module isolation** (`../rules/architecture.md`):
   - `{Module}.Domain/Application/Infrastructure/Presentation` references only `{Other}.Contracts` of other modules.
   - No cross-module `DbContext` access. No cross-module navigation properties. No `IQueryable` leaks.
   - Contracts projects expose DTOs / events / reader interfaces only — no EF entities, no Domain entities, no `IQueryable`.
2. **Clean Architecture layering** (`../rules/architecture.md`):
   - Domain references only `BuildingBlocks.SharedKernel` (and `BuildingBlocks.Domain` if present).
   - Application references Domain and `BuildingBlocks.Application` only.
   - Infrastructure may implement Application abstractions.
   - Presentation depends on Application only — no Infrastructure.
3. **AI isolation** (`../rules/ai.md`):
   - Only `AI.Infrastructure` references AI provider SDKs (Gemini/OpenAI/DeepSeek packages).
   - Other modules consume `AI.Contracts`.
4. **Outbox/Inbox usage** (`../rules/events.md`):
   - Any new integration event is produced via the producer's outbox in the same transaction as business data.
   - Any new consumer checks Inbox before processing.
5. **Central Package Management** (`../rules/packages.md`):
   - No `Version=` attribute in any `.csproj` change.
   - No duplicate `TargetFramework`/`Nullable`/`ImplicitUsings` declarations.
6. **Architecture tests update**: when a new boundary or rule is introduced, add the corresponding NetArchTest assertion in `tests/EnglishTutor.ArchitectureTests`.

## Output

- `tests/EnglishTutor.ArchitectureTests/...` — updated assertions.
- `docs/adr/{NNNN}-{slug}.md` — for ADR-level decisions.

## Commands

```bash
cd EnglishTutor.API
dotnet build EnglishTutor.slnx
dotnet test tests/EnglishTutor.ArchitectureTests
```

## Done when

- `dotnet test tests/EnglishTutor.ArchitectureTests` is green.
- New rule (if any) has a matching assertion.
- ADR added if the decision is non-trivial.
