# EnglishLiveTutor backend — Identity, RBAC, DB prompts and teacher voices

**Architecture:** Pragmatic Clean Architecture, Domain aggregates + feature-oriented application. This package is based on the latest `backend(1).zip` and retains its replacement `20261014182448_InitialCreated` migration. No old migration was rewritten or deleted.

## Required before startup

The API now refuses to start without a JWT HMAC key of **at least 32 UTF-8 bytes**. Do not commit keys/passwords to appsettings or Git.

From `backend/`:

```powershell
# Replace the placeholder with a freshly generated random secret.
# For example, generate one securely using PowerShell:
$key = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
dotnet user-secrets init --project src/EnglishLiveTutor.Api/EnglishLiveTutor.Api.csproj
dotnet user-secrets set "Jwt:Key" "$key" --project src/EnglishLiveTutor.Api/EnglishLiveTutor.Api.csproj
# Bootstrap Admin: set both values once (never stored in source).
dotnet user-secrets set "Auth:BootstrapAdmin:Email" "admin@example.com" --project src/EnglishLiveTutor.Api/EnglishLiveTutor.Api.csproj
dotnet user-secrets set "Auth:BootstrapAdmin:Password" "YOUR_LONG_RANDOM_PASSWORD" --project src/EnglishLiveTutor.Api/EnglishLiveTutor.Api.csproj
```

In production, use environment variables `Jwt__Key`, `Auth__BootstrapAdmin__Email` and `Auth__BootstrapAdmin__Password` or your secrets manager. Omit bootstrap credentials after first initialization. The bootstrap seeder does **not** change existing user passwords.

## Database and migrations

New migration: `20261015010000_AddIdentityPromptsTeacherVoices`.

Adds **8 tables** without modifying original tables:

- `Users`, `Roles`, `Permissions`, `UserRoles`, `RolePermissions`, `RefreshTokens`
- `AiPromptTemplates`, `TeacherVoices`

**Important:** Before applying to any non-throwaway SQL Server database, back it up, inspect the migration SQL, and verify `__EFMigrationsHistory` matches your latest `InitialCreated` migration. This source was NOT tested against a live DB.

```powershell
dotnet restore EnglishLiveTutor.slnx
dotnet build EnglishLiveTutor.slnx
dotnet test EnglishLiveTutor.slnx

dotnet ef migrations list --project src/EnglishLiveTutor.Infrastructure --startup-project src/EnglishLiveTutor.Api --context AppDbContext

dotnet ef migrations script --project src/EnglishLiveTutor.Infrastructure --startup-project src/EnglishLiveTutor.Api --context AppDbContext --idempotent --output migration-review.sql
```

`Program.cs` still automatically calls `MigrateAsync()` and the existing `DatabaseSeeder` **only in Development**. `ApplicationCatalogSeeder` runs afterward, adding missing prompts, teacher voices, initial permissions/roles and optional bootstrap Admin.

For Production, apply reviewed migration scripts in your deployment pipeline, then execute an explicit catalog-seeding step under an authorized deployment account. The current Program intentionally does not auto-migrate/seed Production.

The old `DatabaseSeeder` still has legacy conditional `CREATE TABLE` SQL inherited from your project; it was kept intact to preserve your custom migration changes. Long-term, move all schema changes exclusively to EF migrations.

## Identity API

Authentication (public unless noted):

| Method | Path | Purpose |
|---|---|---|
| POST | `/api/auth/register` | Create Student user; `{email,displayName,password}` |
| POST | `/api/auth/login` | `{email,password}` → user, access + refresh token |
| POST | `/api/auth/refresh` | `{refreshToken}` → rotated refresh token + new access token |
| POST | `/api/auth/logout` | `{refreshToken}` → revoke token |
| GET | `/api/auth/me` | Bearer JWT required |

Administration (JWT + permission claim):

| Method | Path | Permission |
|---|---|---|
| GET / POST | `/api/admin/users` | `users.read` / `users.manage` |
| PUT | `/api/admin/users/{id}/enabled` | `users.manage` |
| PUT | `/api/admin/users/{id}/roles/{role}` | `users.manage` |
| GET / POST / DELETE | `/api/admin/roles`, `/api/admin/roles/{name}` | `roles.read`, `roles.manage` |
| GET / POST / DELETE | `/api/admin/permissions`, `/api/admin/permissions/{code}` | `roles.read`, `roles.manage` |
| PUT | `/api/admin/roles/{role}/permissions/{code}` | `roles.manage` |

Put `{ "assigned": true }` to add a role; `{ "granted": true }` to grant a permission. Built-in roles and permissions cannot be deleted. The last active admin cannot be disabled or stripped of Admin membership. Role/permission changes invalidate stale JWTs during token validation.

Passwords use ASP.NET Core Identity `PasswordHasher` (salted adaptive PBKDF2). Access tokens expire after 15 minutes. Refresh tokens are 64 cryptographically random bytes, stored as SHA-256 hashes and expire after 14 days. Rotating a refresh token atomically marks the previous token revoked; reuse revokes other sessions for that user. HTTP auth endpoints are rate-limited by IP. Use HTTPS and protect refresh tokens on the frontend (prefer secure HttpOnly cookies at your BFF layer; this API currently returns them in JSON).

**Security scope:** Existing Conversation, Vocabulary, Learning, Pronunciation endpoints and SignalR hubs retain their previous anonymous access patterns so this change does not silently break the existing FE. They are **NOT yet per-user secured**: before production use with multiple users, add user ownership, authorization and resource-level checks (and migrate existing `default-learner` data appropriately). Protect Gemini API keys; never log them. New admin/config endpoints are permission protected.

## CEFR prompt catalog

- `AiPromptTemplateEntity` is its own aggregate.
- Seeder inserts 6 CEFR level prompts, 19 session modes plus fallback, 5 speed presets, 2 thinking modes, role template and rules template.
- The original session-mode directives were preserved in the **seed catalog**; `CefrPromptBuilder` now reads the current template text from SQL Server for **every new tutor session**, not from the old switch statement.
- Seeder uses insert-if-missing; **editing a template via API will not be overwritten on startup**.

| Method | Path | Permission |
|---|---|---|
| GET | `/api/ai-prompts` | `prompts.read` |
| GET | `/api/ai-prompts/{code}` | `prompts.read` |
| PUT | `/api/ai-prompts/{code}` | `prompts.manage` |

Example PUT `/api/ai-prompts/cefr.B1`:

```json
{ "content": "Use friendly everyday English for intermediate learners.", "isEnabled": true }
```

Important template keys: `cefr.A1` ... `cefr.C2`, `mode.technical_interview`, `mode.practice_interview`, `mode.daily_conversation`, `speed.normal`, `thinking.extended`, `common.role`, `common.rules`.

## Google Teacher Voice catalog

All **30** Google Gemini Live prebuilt voices supplied in the task are seeded. `Aoede`, `Puck`, `Kore`, `Charon`, `Fenrir`, `Leda` have Vietnamese labels, style descriptions and `isRecommended: true`. Voice codes are case-sensitive.

| Method | Path | Auth |
|---|---|---|
| GET | `/api/teacher-voices` | Public, with `isRecommended` and `isEnabled` |
| PUT | `/api/teacher-voices/{code}/enabled` | `voices.manage` |

Example list item:

```json
{
  "code": "Aoede", "id": "Aoede", "name": "Aoede", "displayName": "Aoede",
  "type": "prebuilt", "gender": "Female", "persona": "Breezy",
  "labelVi": "Aoede (Nữ)", "styleVi": "Truyền cảm, ấm áp & rõ chữ",
  "isRecommended": true, "isEnabled": true
}
```

The existing `StartTutorSessionRequest.VoiceName` remains unchanged. Sessions and conversation create/update validate against the enabled database catalog and preserve `speechConfig.voiceConfig.prebuiltVoiceConfig.voiceName`. Available voices can differ by Gemini model; failed WebSocket setup from an unsupported model-specific voice is still possible.

Google documentation: <https://ai.google.dev/gemini-api/docs/live-api/capabilities>

## Verification status and limitations

- Source changes and migration metadata were reviewed statically.
- New Domain tests added: `IdentityAndCatalogDomainTests`.
- **No `dotnet` SDK and no SQL Server were available in the editing environment. Build, tests, migration execution and Live WebSocket integration were not run.**
- This ZIP is a development/refactor deliverable, **not a production-validated release**.
- Existing legacy schema setup in `DatabaseSeeder` and duplicated query-side read paths were intentionally not broadly refactored.
- Runtime JWT signing key configuration is mandatory even in Development; set it before running or using `dotnet ef` with the API startup project.
