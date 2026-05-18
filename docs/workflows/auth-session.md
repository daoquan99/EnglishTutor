# Auth Session Workflow

## Overview

Auth uses short-lived access tokens and rotating refresh tokens bound to an auth session and device id. Raw refresh tokens are stored only in `HttpOnly`, `Secure` cookies. The database stores hashed refresh tokens as the source of truth, while Redis stores the current session/device token hash as a fast cache.

## Main Flow

1. Register or login creates an `AuthSession`.
2. Auth generates a raw refresh token and stores only its hash in `auth.RefreshTokens`.
3. Auth stores the same hash in Redis under `auth:refresh:{sessionId}:{deviceId}` with the refresh-token expiry as TTL.
4. API returns the access token in the response body and sets refresh/session/device cookies.
5. Refresh validates the session/device binding, hashes the cookie refresh token, validates the DB token, rotates the refresh token, and overwrites the Redis key.
6. Logout revokes the current token/session, removes the Redis key, and clears cookies.

## Detailed Steps

- `POST /api/auth/register` creates `AuthUser`, `UserCredential`, `AuthSession`, `RefreshToken`, and `UserRegisteredIntegrationEvent`.
- `POST /api/auth/login` validates credentials and creates a new `AuthSession` plus `RefreshToken`.
- `POST /api/auth/refresh-token` reads `et_refresh_token`, `et_session_id`, and `et_device_id` cookies.
- Refresh hashes the raw cookie token before any persistence lookup.
- Refresh rejects missing, revoked, expired, or mismatched token/session/device combinations.
- On successful refresh, Auth marks the old token revoked, creates a replacement token row, overwrites the Redis cache value, and sets a new refresh cookie.
- If a revoked token is reused or a token hash does not match the current session, Auth records `AuthSecurityEvent`, marks the session suspicious, revokes the session, removes Redis cache, and forces a new login.

## Modules Involved

- Auth
- Users
- Worker

## Contracts Used

- `UserRegisteredIntegrationEvent`

## Events Published/Consumed

- Published by Auth: `UserRegisteredIntegrationEvent`.
- Consumed by Users: `UserRegisteredIntegrationEvent`.

## Read Models/Projections Updated

None directly.

## Failure/Retry Behavior

- Redis is a cache only; the database remains the source of truth for refresh-token validity.
- Redis failures should not issue false security incidents by themselves.
- Reused revoked tokens and session/token mismatches are treated as suspicious activity and persisted in `AuthSecurityEvents` for admin review.
- Integration events are saved to the producer outbox and dispatched by Worker with retry/dead-letter handling.
