# Multi-language rules

The platform supports many target languages without modeling them as tenants.

## Four language codes

| Code                     | Meaning                                                      | Example |
| ------------------------ | ------------------------------------------------------------ | ------- |
| `NativeLanguageCode`     | Learner's native language. Used for translations / explanations. | `vi`    |
| `UiLanguageCode`         | Language of menus and UI strings.                            | `en`    |
| `ExplanationLanguageCode`| Language the AI uses to explain grammar/vocab.               | `vi`    |
| `TargetLanguageCode`     | Language the user is actively learning.                      | `en`    |

The `Users` module owns these settings. Other modules read them via `Users.Contracts` (Contract Reader).

## Scoping

Per-user, per-language data is scoped by the composite key:

```
UserId + TargetLanguageCode
```

Applies to (at minimum): mastery records, exercise attempts, mistakes, progress (EXP, streak), speaking session summaries, daily/weekly/monthly snapshots.

Speaking sessions snapshot the language pair (`NativeLanguageCode`, `TargetLanguageCode`, `ExplanationLanguageCode`, level) at session start — those values are stored on the session aggregate, not re-read from Users each time.

## AI requests

Every AI request includes native + target + explanation language and current proficiency level. See `ai.md`.

## Timezone vs language

Language is **not** timezone.

For daily-boundary work (streaks, daily progress, daily reminders): Application computes the date boundary from the user's timezone setting (also in Users) and persists the business date. Underlying timestamps stay UTC. See `domain-modeling.md`.

## Frontend display

Backend always returns UTC timestamps. Frontend converts to the user's display timezone. Backend never returns local-time strings or pre-formatted dates.

## Forbidden

- Introducing a `TenantId` for language. Language is **not** tenant.
- Storing user progress without `TargetLanguageCode` in the key.
- Persisting the user's display timezone offset into business records — store the user's timezone *setting* once, in Users.
- Pre-formatting timestamps server-side for display.
- Sending AI requests without language + level context.
