# Auth Module

## Responsibility
Owns identity and authentication.

## Schema
`auth`

## Tables
- `auth.Users` — Identity user (Id, Email, DisplayName, IsActive)
- `auth.RefreshTokens` — JWT refresh tokens
- `auth.UserCredentials` — Hashed passwords

## Features
- Register, Login, Refresh token, Logout
- Password hashing (BCrypt)
- JWT generation (HMAC-SHA256)
- Role/permission basics (future)

## Must NOT Own
- User learning profile, language settings, learning goals, study schedule

## Endpoints
| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/auth/register` | No |
| POST | `/api/auth/login` | No |
| POST | `/api/auth/refresh-token` | No |
| POST | `/api/auth/logout` | Yes |
| GET | `/api/auth/me` | Yes |

## Integration Events Produced
- `UserRegisteredIntegrationEvent` (UserId, Email, DisplayName, RegisteredAtUtc)

## Contracts
- `AuthUserBasicInfo` read model
