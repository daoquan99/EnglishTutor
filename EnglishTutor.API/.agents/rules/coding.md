# Coding rules

## Language & style

- C# latest stable (net10.0, `LangVersion=latest` via `Directory.Build.props`).
- `Nullable` and `ImplicitUsings` are enabled solution-wide. Don't redeclare in `.csproj`.
- Follow existing naming and folder conventions. Don't rename public APIs unless required.
- Methods small and readable. Prefer explicit names over generic abstractions.
- Use `async`/`await` for IO. Always accept and pass `CancellationToken` through Application handlers and Infrastructure calls.

## What not to add

- Don't introduce generic repositories unless the project already uses them. Repositories exist for aggregate roots only.
- Don't create large "god" services. Don't add abstractions for hypothetical future requirements.
- Don't add error-handling, retries, or validation for cases that cannot occur — trust internal callers and framework guarantees; validate only at system boundaries.
- Don't add backwards-compat shims, feature flags, or `_unused` parameter renames in lieu of cleanup.
- Don't write planning, decision, or analysis markdown files unless explicitly asked.

## Comments & docs

- Default to no comments. Only add one when the **why** is non-obvious — hidden constraint, subtle invariant, workaround for a specific bug, behavior that would surprise a reader.
- Don't explain **what** the code does — names should carry that.
- Don't reference current tasks/PRs/callers ("used by X", "added for the Y flow") — that belongs in the PR description.
- XML doc comments only where they add real value to public Contracts surface.

## Result/CancellationToken

- Application handlers return `Result` or `Result<T>` for expected outcomes (not found, conflict, validation, etc.). See `error-handling.md`.
- Application handler signature: `Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)`.
- Do not throw for expected use-case failures.

## DateTime

See `multi-language.md` (timezone) and `domain-modeling.md` (UTC + `Utc` suffix).

- Never use `DateTime.Now`.
- Use `IDateTimeProvider.UtcNow` in Application/Infrastructure.
- Domain methods that need current time should receive UTC `DateTime` from the caller when practical.

## Package versions

See `packages.md`. Versions go in `Directory.Packages.props` only. Projects use bare `<PackageReference Include="X" />`.
