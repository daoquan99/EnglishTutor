# Error handling rules

Domain and Application have different roles. Don't mix them.

## Domain — invariant guardrails

Domain throws when the aggregate would otherwise enter an invalid state.

- `BusinessRuleValidationException` — thrown by `CheckRule(...)` when an `IBusinessRule` is broken. Use this when the rule has a name and stable code.
- `DomainException` — ad-hoc invariant violation (no named rule needed).

Domain must **not**:

- Reference `BuildingBlocks.Application`.
- Use `Result` / `Result<T>` / `Error` from Application.
- Define module-specific response error records like `AuthError` whose only job is to carry `Code` + `Message`.

Domain exceptions are guardrails. They should be rare at runtime and indicate a caller bug, a race condition, or invariant breach — not a normal use-case branch.

## Application — expected outcomes

Application handlers return `Result` / `Result<T>` for everything the user might trigger:

- Not found
- Invalid credentials
- Conflict (duplicate, version mismatch)
- Validation failure
- Forbidden by business rules ("can't review a card that's not due")

Module-specific error catalogs live in `{Module}.Application/Shared/Errors/` (e.g., `AuthErrors`, `VocabularyErrors`). They use `EnglishTutor.BuildingBlocks.Application.Results.Error`.

Error catalog convention:

```csharp
public static class VocabularyErrors
{
    public static readonly Error CardNotFound =
        new("Vocabulary.CardNotFound", "Vocabulary card not found.");

    public static Error MasteryRequirementNotMet(int required, int actual) =>
        new("Vocabulary.MasteryRequirementNotMet",
            $"Required {required} correct answers; got {actual}.");
}
```

Codes are stable, dotted (`Module.ErrorName`), and documented in `docs/api/{module}.md`.

## Presentation mapping

- `Result.Success` → `200 OK` / `204 NoContent` / `201 Created` (Presentation chooses based on operation).
- `Result.Failure` with an `Error` → mapped to HTTP via the project's `ResultExtensions`. Typical mappings:
  - `*.NotFound` → 404
  - `*.Unauthorized` → 401
  - `*.Forbidden` → 403
  - `*.Conflict` → 409
  - validation / business-rule errors → 400 / 422 with `ProblemDetails`

## Global exception handler

`GlobalExceptionHandlerMiddleware` catches:

- `BusinessRuleValidationException` / `DomainException` → 400 (or 422) with `ProblemDetails` carrying the rule code.
- Validation pipeline exceptions → 400 with field-level errors.
- Anything else → 500 with a correlation ID; logged.

## Forbidden

- Throwing for expected use-case failures (return `Result.Failure(Error)` instead).
- Returning `Result` from Domain methods.
- Creating module-local error record types in Domain (use `Error` in Application).
- Hard-coding error message strings in Presentation — pull from the Application catalog.
- Swallowing exceptions in handlers.
