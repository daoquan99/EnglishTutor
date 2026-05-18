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
| `POST /api/auth/logout` | Revoke the current auth session. | Required |
| `GET /api/auth/me` | Return current authenticated user info. | Required |

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

## Response Bodies

Token endpoints return:

```json
{ "accessToken": "jwt", "expiresAtUtc": "2026-05-18T10:00:00Z" }
```

Register, login, and refresh also set these `HttpOnly`, `Secure`, `SameSite=Strict` cookies scoped to `/api/auth`:

- `et_refresh_token`: raw refresh token, never returned in JSON.
- `et_session_id`: auth session id.
- `et_device_id`: device/session binding value.

`GET /api/auth/me` returns:

```json
{ "userId": "00000000-0000-0000-0000-000000000000", "email": "learner@example.com", "displayName": "Learner" }
```

## Validation Rules

- Email is required, valid, normalized lowercase, max 256 chars.
- Register password is required, at least 8 chars, and must match `confirmPassword`.
- Display name is required and constrained by domain validation.
- Refresh/logout require refresh, session, and device cookies.
- Refresh tokens are stored as hashes in the database and Redis cache.

## Error Codes

- `Error.Validation`: invalid request data.
- `Error.Conflict`: email already exists.
- `Error.NotFound`: user or refresh token not found.
- `Error.Unauthorized`: invalid credentials, expired/revoked token, or suspicious refresh session.
- `Error.Forbidden`: inactive user.

## Application Flow

- Register creates `AuthUser`, `UserCredential`, `AuthSession`, hashed refresh token, JWT, Redis refresh cache entry, and an outbox message.
- Login validates password through `IPasswordHasher`, creates a new `AuthSession`, and issues a new access token plus refresh cookie.
- Refresh hashes the raw refresh cookie, checks session/device binding, validates the token in DB, rotates the refresh token, overwrites `auth:refresh:{sessionId}:{deviceId}` in Redis, and sets a new refresh cookie.
- If a revoked refresh token is reused, or a token hash cannot be matched to the session, Auth records an `AuthSecurityEvent`, marks/revokes the session, clears Redis, and forces login again.
- Logout checks the session belongs to the current user, revokes the refresh token and session, removes the Redis key, and clears cookies.

## Related Modules

- Auth owns identity credentials and tokens.
- Users consumes registration events to create profile/language defaults.

## Integration Events Produced

- `UserRegisteredIntegrationEvent`

## Integration Events Consumed

- None.

## Read Models/Projections Updated

- None directly.
