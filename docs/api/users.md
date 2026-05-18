# Users API

All endpoints require authentication and use `ICurrentUser.UserId`.

## Endpoints

| Endpoint | Purpose |
| --- | --- |
| `GET /api/users/me/profile` | Get current user profile. |
| `PUT /api/users/me/profile` | Update display name, avatar, and bio. |
| `GET /api/users/me/language-settings` | Get native/UI/explanation/active target language settings. |
| `PUT /api/users/me/language-settings` | Update language settings. |
| `GET /api/users/me/target-languages` | List target languages. |
| `POST /api/users/me/target-languages` | Add a target language. |
| `PUT /api/users/me/target-languages/{id}/activate` | Activate one target language. |

## Request Bodies

```json
{ "displayName": "Learner", "avatarUrl": "https://example.com/avatar.png", "bio": "English learner" }
```

```json
{ "nativeLanguageCode": "vi", "uiLanguageCode": "vi", "explanationLanguageCode": "vi", "activeTargetLanguageCode": "en" }
```

```json
{ "targetLanguageCode": "en", "currentLevel": "A1", "targetLevel": "B2" }
```

## Response Bodies

Profile:

```json
{ "userId": "00000000-0000-0000-0000-000000000000", "displayName": "Learner", "avatarUrl": null, "bio": null }
```

Language settings:

```json
{ "nativeLanguageCode": "vi", "uiLanguageCode": "vi", "explanationLanguageCode": "vi", "activeTargetLanguageCode": "en" }
```

Target language:

```json
{ "id": "00000000-0000-0000-0000-000000000000", "targetLanguageCode": "en", "currentLevel": "A1", "targetLevel": "B2", "isActive": true }
```

## Validation Rules

- Display name is required and domain-normalized.
- Language codes must be valid 2-3 character language codes.
- Language levels must map to `A1`, `A2`, `B1`, `B2`, `C1`, or `C2`.
- A user can only have one active target language.

## Error Codes

- `Error.Validation`: invalid input or invalid state.
- `Error.NotFound`: profile, settings, or target language not found.
- `Error.Conflict`: target language already exists.

## Application Flow

- `UserRegisteredIntegrationEvent` creates default profile, language settings, and English target language through Inbox-protected handler.
- Profile and language updates are saved in Users schema and published through Users Outbox.
- API routes delegate to Application commands/queries only.

## Related Modules

- Auth produces registration events.
- Speaking and AI use Users.Contracts readers for language context.
- Progress may consume language/level events for projections.

## Integration Events Produced

- `UserProfileUpdatedIntegrationEvent`
- `UserLanguageSettingsUpdatedIntegrationEvent`
- `UserTargetLanguageChangedIntegrationEvent`
- `UserLevelChangedIntegrationEvent`

## Integration Events Consumed

- `UserRegisteredIntegrationEvent`

## Read Models/Projections Updated

- Users-owned contract read models for profile, language settings, and target languages.
