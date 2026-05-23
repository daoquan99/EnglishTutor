# Users module

User profile, language preferences, and active target language. Distinct from Auth (which owns identity & credentials).

## Schema

`users`

## Aggregate roots

| Aggregate                | Purpose                                                                                |
| ------------------------ | -------------------------------------------------------------------------------------- |
| `UserProfile`            | Display name, avatar, timezone, locale, level, profile fields.                         |
| `UserLanguageSettings`   | `NativeLanguageCode`, `UiLanguageCode`, `ExplanationLanguageCode`.                     |
| `UserTargetLanguage`     | Active target language and per-target proficiency level. Many per user, one active.    |

## Contracts surface

- `Readers/` — Contract Reader interfaces (sync small reads). Used by Speaking, Vocabulary, AI, Progress to fetch language/level snapshot.
- `ReadModels/` — DTOs returned by readers (e.g., `UserLanguageContextDto { NativeLanguageCode, TargetLanguageCode, ExplanationLanguageCode, Level }`).
- `IntegrationEvents/`:
  - `UserProfileUpdatedIntegrationEvent`
  - `UserLanguageSettingsUpdatedIntegrationEvent`
  - `UserTargetLanguageChangedIntegrationEvent` — important for Progress/Mistakes scoping.
  - `UserLevelChangedIntegrationEvent` — published when level-up assessment passes.

## Key behaviors

- Listens to `UserRegisteredIntegrationEvent` from Auth to create the profile + default language settings.
- Listens to `AssessmentPassedIntegrationEvent` (level-up) and raises `UserLevelChangedIntegrationEvent`.
- Timezone is stored as IANA name (e.g., `Asia/Ho_Chi_Minh`). Used by Application code to compute calendar-day boundaries; see `../rules/multi-language.md`.

## Notes for changes

- Adding a language code field: bump readers' DTO version and update consumers.
- Language is NOT tenancy — do not introduce `TenantId`. See `../rules/multi-language.md`.
