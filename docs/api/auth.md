# Auth API

All responses use the shared envelope:

```json
{ "isSuccess": true, "data": {}, "error": null }
```

Errors use:

```json
{ "isSuccess": false, "data": null, "error": { "code": "Error.Validation", "message": "..." } }
```

## Endpoints

| Endpoint | Purpose | Auth |
| --- | --- | --- |
| `POST /api/auth/register` | Register a new auth user and issue tokens. | Anonymous |
| `POST /api/auth/login` | Authenticate by email/password and issue tokens. | Anonymous |
| `POST /api/auth/refresh-token` | Rotate refresh cookie and issue a new access token. | Anonymous |
| `POST /api/auth/logout` | Revoke the current auth session from refresh cookies. | Anonymous |
| `GET /api/auth/me` | Return current authenticated user info. | Required |
| `GET /api/admin/auth/permissions` | List auth permissions. | Permission `auth.permissions.read` |
| `GET /api/admin/auth/permissions/{permissionId}` | Get one auth permission. | Permission `auth.permissions.read` |
| `POST /api/admin/auth/permissions` | Create a custom permission. | Permission `auth.permissions.manage` |
| `PUT /api/admin/auth/permissions/{permissionId}` | Update permission description/enabled state. | Permission `auth.permissions.manage` |
| `DELETE /api/admin/auth/permissions/{permissionId}` | Delete an unassigned custom permission. | Permission `auth.permissions.manage` |
| `GET /api/admin/auth/roles` | List auth roles with assigned permissions. | Permission `auth.roles.read` |
| `GET /api/admin/auth/roles/{roleId}` | Get one auth role with assigned permissions. | Permission `auth.roles.read` |
| `POST /api/admin/auth/roles` | Create a custom role and assign permissions. | Permission `auth.roles.manage` |
| `PUT /api/admin/auth/roles/{roleId}` | Update a custom role and synchronize permissions. | Permission `auth.roles.manage` |
| `DELETE /api/admin/auth/roles/{roleId}` | Soft-delete an unassigned custom role. | Permission `auth.roles.manage` |

## Request Bodies

`POST /api/auth/register`

```json
{ "email": "learner@example.com", "password": "Password123!", "confirmPassword": "Password123!", "displayName": "Learner" }
```

`POST /api/auth/login`

```json
{ "email": "learner@example.com", "password": "Password123!" }
```

`POST /api/auth/refresh-token` and `POST /api/auth/logout` do not use a request body. The browser sends `et_refresh_token`, `et_session_id`, and `et_device_id` cookies.

`POST /api/admin/auth/permissions`

```json
{ "code": "vocabulary.items.export", "description": "Export vocabulary items." }
```

`PUT /api/admin/auth/permissions/{permissionId}`

```json
{ "description": "Export vocabulary items.", "isEnabled": true }
```

`POST /api/admin/auth/roles`

```json
{
  "name": "content_admin",
  "description": "Manage learning content and vocabulary.",
  "isEnabled": true,
  "permissionIds": ["00000000-0000-0000-0000-000000000000"]
}
```

`PUT /api/admin/auth/roles/{roleId}` uses the same body as role creation and replaces the role's permission assignment set.

## Response Bodies

Token endpoints return:

```json
{ "accessToken": "jwt", "expiresAtUtc": "2026-05-18T10:00:00Z" }
```

Register, login, and refresh also set these `HttpOnly` cookies scoped to `/api/auth` by default:

- `et_refresh_token`: raw refresh token, never returned in JSON.
- `et_session_id`: auth session id.
- `et_device_id`: device/session binding value.

Cookie transport is configured by `Auth:Cookies` in API appsettings. The default is `Secure=true`, `SameSite=Lax`, and `Path=/api/auth`. Cross-site browser clients must explicitly configure cookie SameSite behavior together with CORS and CSRF controls.

`GET /api/auth/me` returns:

```json
{
  "userId": "00000000-0000-0000-0000-000000000000",
  "email": "learner@example.com",
  "displayName": "Learner",
  "roles": ["admin"],
  "permissions": ["admin.full_access", "ai.providers.manage"]
}
```

Admin permission endpoints return:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "code": "auth.roles.manage",
  "description": "Manage authentication roles.",
  "isEnabled": true
}
```

Admin role endpoints return:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "name": "content_admin",
  "description": "Manage learning content and vocabulary.",
  "isSystem": false,
  "isEnabled": true,
  "permissions": [
    {
      "id": "00000000-0000-0000-0000-000000000000",
      "code": "vocabulary.items.manage",
      "description": "Manage vocabulary items.",
      "isEnabled": true
    }
  ]
}
```

## Validation Rules

- Email is required, valid, normalized lowercase, max 256 chars.
- Register password is required, at least 8 chars, and must match `confirmPassword`.
- Display name is required and constrained by domain validation.
- Refresh/logout require refresh, session, and device cookies.
- Refresh-token rotation is rate limited by the API host refresh policy.
- Refresh tokens are stored as hashes in the database and Redis cache.
- Register/login/refresh do not issue a valid session if the current refresh-token hash cannot be written to Redis; the created session/token are revoked and the client must retry/login again.
- Runtime authorization checks use permission codes, not role names.

## Browser CORS Requirements

- API CORS is configured from `Cors:AllowedOrigins` and `Cors:AllowCredentials` in appsettings.
- Browser clients must call auth endpoints with credentials enabled so `HttpOnly` cookies can be set and sent.
- `AllowCredentials=true` requires explicit origins; wildcard `*` is rejected by startup configuration.
- Roles only group permissions for assignment.
- Access tokens contain `permission` claims.
- Admin role/permission endpoints check permissions directly; `admin.full_access` is a wildcard.
- `includeDisabled=true` can be passed to role/permission list endpoints.
- Permission code is stable after creation and must be lowercase-normalizable, unique, max 150 chars, and contain only letters, digits, `.`, `-`, or `_`.
- Permission description is required and max 300 chars.
- System permissions from `PermissionCodes.All` cannot be deleted or disabled.
- System permission catalog includes Auth, Users, StudyPlans, LearningContent, Vocabulary, Exercises, Speaking, Mistakes, Assessments, Progress, Notifications, AI, and Reports permissions.
- Role name is stable-normalized lowercase, unique, max 100 chars, and contains only letters, digits, `-`, or `_`.
- Role description is required and max 300 chars.
- Role create/update accepts enabled permission ids only.
- Role update replaces the full permission assignment set.
- System roles, including seeded `admin`, cannot be modified or deleted through the admin CRUD API.
- A custom role assigned to at least one user cannot be deleted.
- A custom permission assigned to at least one role cannot be deleted.

## Error Codes

- `Error.Validation`: invalid request data.
- `Error.Conflict`: email already exists.
- `Error.Conflict`: role/permission code already exists, system role/permission is protected, or the item is still assigned.
- `Error.NotFound`: user or refresh token not found.
- `Error.NotFound`: role or permission not found.
- `Error.Unauthorized`: invalid credentials, expired/revoked token, suspicious refresh session, or unavailable refresh-token verification.
- `Error.Forbidden`: inactive user.
- `Error.Forbidden`: authenticated user lacks the required permission code for an admin endpoint.

## Application Flow

- Register creates `AuthUser`, `UserCredential`, `AuthSession`, hashed refresh token, JWT, Redis refresh cache entry, and an outbox message.
- Login validates password through `IPasswordHasher`, loads permission codes through Auth role assignments, creates a new `AuthSession`, and issues a new access token plus refresh cookie.
- Refresh hashes the raw refresh cookie, checks session/device binding, requires Redis `auth:refresh:{sessionId}:{deviceId}` hash match, validates the token in DB, rotates the refresh token, overwrites Redis, and sets a new refresh cookie.
- If a revoked refresh token is reused, or a token hash cannot be matched to the session, Auth records an `AuthSecurityEvent`, marks/revokes the session, clears Redis, and forces login again.
- Redis read failure fails closed without creating a security incident; Redis cache miss or hash mismatch is treated as suspicious.
- Logout checks the session belongs to the current user, revokes the refresh token and session, removes the Redis key, and clears cookies.
- Permission admin flow lists, creates, updates, or deletes custom permissions inside Auth. Deleting is blocked for system permissions and assigned permissions.
- Role admin flow lists, creates, updates, or soft-deletes custom roles inside Auth. Role update synchronizes the role-permission join table to match the request.

## Related Modules

- Auth owns identity credentials and tokens.
- Users consumes registration events to create profile/language defaults.

## Integration Events Produced

- `UserRegisteredIntegrationEvent`

## Integration Events Consumed

- None.

## Read Models/Projections Updated

- None directly.

## Seed Data

- When `SeedData:Enabled=true`, API startup seeds Auth permissions, an `admin` role, role-permission mappings, and an admin user.
- Default development admin config:
  - `SeedData:Admin:Email=admin@englishtutor.local`
  - `SeedData:Admin:Password=Admin@123456`
  - `SeedData:Admin:DisplayName=System Admin`
- The admin role receives every known permission, including `admin.full_access`.
- Existing admin users are activated and missing credentials/assignments are repaired, but existing passwords are not overwritten.
