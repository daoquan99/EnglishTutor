# Users Module

## Responsibility
Owns user profile, language settings, and target languages.

## Schema
`users`

## Tables
- `users.UserProfiles` — DisplayName, AvatarUrl, Bio
- `users.UserLanguageSettings` — NativeLanguageCode, UiLanguageCode, ExplanationLanguageCode, ActiveTargetLanguageCode
- `users.UserTargetLanguages` — TargetLanguageCode, CurrentLevel, TargetLevel, IsActive
- `users.UserPreferences` — Daily preferences

## Endpoints
| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/users/me/profile` | Yes |
| PUT | `/api/users/me/profile` | Yes |
| GET | `/api/users/me/language-settings` | Yes |
| PUT | `/api/users/me/language-settings` | Yes |
| GET | `/api/users/me/target-languages` | Yes |
| POST | `/api/users/me/target-languages` | Yes |
| PUT | `/api/users/me/target-languages/{id}/activate` | Yes |

## Contract Readers
- `IUserProfileReader` → `UserProfileReadModel`
- `IUserLanguageSettingsReader` → `UserLanguageSettingsReadModel`
- `IUserTargetLanguageReader` → `UserTargetLanguageReadModel`

## Integration Events Produced
- `UserProfileUpdatedIntegrationEvent`
- `UserLanguageSettingsUpdatedIntegrationEvent`
- `UserTargetLanguageChangedIntegrationEvent`
- `UserLevelChangedIntegrationEvent` (UserId, TargetLanguageCode, PreviousLevel, NewLevel)

## Events Consumed
- `UserRegisteredIntegrationEvent` → auto-create UserProfile + default language settings
- `LevelUpApprovedIntegrationEvent` → update UserTargetLanguage.CurrentLevel
