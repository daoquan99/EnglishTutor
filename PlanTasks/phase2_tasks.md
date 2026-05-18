# Phase 2: Identity & User Context — Detailed Tasks

> **Goal:** Auth module (register, login, JWT) + Users module (profile, language settings, target languages, contracts)
> **Dependencies:** Phase 1
> **Estimated tasks:** 14

---

## Task 2.1: Auth.Domain — Identity Entities

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Auth/EnglishTutor.Modules.Auth.Domain/
├── Entities/
│   ├── AuthUser.cs
│   ├── RefreshToken.cs
│   └── UserCredential.cs
├── Events/
│   └── UserRegisteredDomainEvent.cs
├── ValueObjects/
│   ├── Email.cs
│   └── HashedPassword.cs
```

**AuthUser (AggregateRoot<Guid>):**
- `Email` (ValueObject), `DisplayName`, `IsActive`, `CreatedAtUtc`
- Factory method: `static AuthUser Register(Email email, string displayName)`
- Raises `UserRegisteredDomainEvent`

**RefreshToken (Entity<Guid>):**
- `AuthUserId`, `Token`, `ExpiresAtUtc`, `CreatedAtUtc`, `RevokedAtUtc`, `IsRevoked`, `IsExpired`
- Method: `Revoke()`

**UserCredential (Entity<Guid>):**
- `AuthUserId`, `HashedPassword`, `CreatedAtUtc`, `UpdatedAtUtc`

**Email (ValueObject):**
- Validates format, max 256 chars, lowercase normalization

**Acceptance Criteria:**
- [ ] AuthUser raises domain event on registration
- [ ] Email value object validates format
- [ ] RefreshToken tracks revocation state
- [ ] No infrastructure dependencies
- [ ] Domain does not reference Application `Result` or `Error`

---

## Task 2.2: Auth.Application — Commands & Queries

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Auth/EnglishTutor.Modules.Auth.Application/
├── Commands/
│   ├── Register/
│   │   ├── RegisterCommand.cs
│   │   ├── RegisterCommandHandler.cs
│   │   └── RegisterCommandValidator.cs
│   ├── Login/
│   │   ├── LoginCommand.cs
│   │   ├── LoginCommandHandler.cs
│   │   └── LoginCommandValidator.cs
│   ├── RefreshToken/
│   │   ├── RefreshTokenCommand.cs
│   │   └── RefreshTokenCommandHandler.cs
│   └── Logout/
│       ├── LogoutCommand.cs
│       └── LogoutCommandHandler.cs
├── Queries/
│   └── GetCurrentUser/
│       ├── GetCurrentUserQuery.cs
│       └── GetCurrentUserQueryHandler.cs
├── Abstractions/
│   ├── IAuthRepository.cs
│   ├── IPasswordHasher.cs
│   ├── IJwtTokenGenerator.cs
│   └── IRefreshTokenRepository.cs
├── Errors/
│   └── AuthErrors.cs
└── DTOs/
    ├── AuthTokenResponse.cs
    └── CurrentUserResponse.cs
```

**AuthErrors:**
- Lives in `Auth.Application`, not `Auth.Domain`
- Uses `EnglishTutor.BuildingBlocks.Application.Results.Error`
- Includes `InvalidCredentials`, `EmailAlreadyExists`, `RefreshTokenExpired`, `RefreshTokenRevoked`, `RefreshTokenNotFound`, `UserNotFound`, `UserInactive`

**RegisterCommand:** `Email`, `Password`, `ConfirmPassword`, `DisplayName`
**RegisterCommandHandler flow:**
1. Validate email uniqueness via `IAuthRepository`
2. Hash password via `IPasswordHasher`
3. Create `AuthUser.Register()`
4. Save `UserCredential`
5. Generate JWT + RefreshToken
6. Return `AuthTokenResponse`

**LoginCommand:** `Email`, `Password`
**LoginCommandHandler flow:**
1. Find user by email
2. Verify password via `IPasswordHasher`
3. Generate new JWT + RefreshToken
4. Return `AuthTokenResponse`

**RefreshTokenCommand:** `RefreshToken`
**Flow:** Validate token → revoke old → issue new pair

**LogoutCommand:** Revoke current refresh token

**AuthTokenResponse:** `AccessToken`, `RefreshToken`, `ExpiresAtUtc`

**Validators (FluentValidation):**
- Register: email required+valid, password min 8 chars + complexity, confirmPassword must match, displayName required 2-50 chars
- Login: email required, password required

**Acceptance Criteria:**
- [ ] All commands use Result pattern
- [ ] Validators prevent invalid input before handler runs
- [ ] Token generation is abstracted (not in handler)
- [ ] CancellationToken on all async methods

---

## Task 2.3: Auth.Infrastructure — DbContext, JWT, Hashing

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Auth/EnglishTutor.Modules.Auth.Infrastructure/
├── Persistence/
│   ├── AuthDbContext.cs
│   ├── Configurations/
│   │   ├── AuthUserConfiguration.cs
│   │   ├── RefreshTokenConfiguration.cs
│   │   └── UserCredentialConfiguration.cs
│   ├── Repositories/
│   │   ├── AuthRepository.cs
│   │   └── RefreshTokenRepository.cs
│   └── Migrations/ (auto-generated)
├── Authentication/
│   ├── JwtTokenGenerator.cs
│   ├── JwtOptions.cs
│   └── PasswordHasher.cs
├── DependencyInjection.cs
└── OutboxIntegration/
    └── AuthDomainEventToOutboxMapper.cs
```

**AuthDbContext:**
- Schema: `"auth"`
- MigrationsHistoryTable: `"__EFMigrationsHistory"` in `"auth"` schema
- DbSets: `AuthUsers`, `RefreshTokens`, `UserCredentials`

**EF Configurations:**
- `AuthUser`: table `auth.Users`, Email unique index, DisplayName max 100
- `RefreshToken`: table `auth.RefreshTokens`, index on `(AuthUserId)`, index on `(Token)`
- `UserCredential`: table `auth.UserCredentials`, one-to-one with AuthUser

**JwtTokenGenerator : IJwtTokenGenerator:**
- Generate JWT with claims: `sub` (userId), `email`, `display_name`
- Configurable expiry from `JwtOptions`
- Sign with HMAC-SHA256 (dev) — RSA for prod later

**PasswordHasher : IPasswordHasher:**
- Use `BCrypt.Net` or `Microsoft.AspNetCore.Identity.PasswordHasher`
- `string Hash(string password)`
- `bool Verify(string password, string hashedPassword)`

**DependencyInjection:**
- Register `AuthDbContext` with PostgreSQL
- Register repositories, JWT generator, password hasher
- Register MediatR handlers from Application assembly

**Acceptance Criteria:**
- [ ] Schema is `auth`
- [ ] Migration history table in `auth` schema
- [ ] JWT tokens are valid and parseable
- [ ] Passwords are never stored in plain text
- [ ] DI wires all services correctly

---

## Task 2.4: Auth.Presentation — API Endpoints

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Auth/EnglishTutor.Modules.Auth.Presentation/
├── AuthEndpoints.cs
└── Requests/
    ├── RegisterRequest.cs
    ├── LoginRequest.cs
    └── RefreshTokenRequest.cs
```

**Endpoints (Minimal API):**

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/auth/register` | Anonymous | Register new user |
| POST | `/api/auth/login` | Anonymous | Login |
| POST | `/api/auth/refresh-token` | Anonymous | Refresh JWT |
| POST | `/api/auth/logout` | Authenticated | Revoke refresh token |
| GET | `/api/auth/me` | Authenticated | Get current user info |

**Each endpoint:**
- Maps request DTO → Command/Query
- Sends via MediatR
- Returns `Result.ToHttpResult()`
- Has unique operation ID for Swagger

**Acceptance Criteria:**
- [ ] 5 endpoints mapped
- [ ] Anonymous endpoints don't require auth
- [ ] Protected endpoints return 401 without JWT
- [ ] Request DTOs are separate from commands
- [ ] No business logic in endpoints

---

## Task 2.5: Auth.Contracts

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Auth/EnglishTutor.Modules.Auth.Contracts/
├── IntegrationEvents/
│   └── UserRegisteredIntegrationEvent.cs
└── ReadModels/
    └── AuthUserBasicInfo.cs
```

**UserRegisteredIntegrationEvent : IntegrationEvent:**
- `Guid UserId`
- `string Email`
- `string DisplayName`
- `DateTime RegisteredAtUtc`

**AuthUserBasicInfo:**
- `Guid UserId`, `string Email`, `string DisplayName`

**Acceptance Criteria:**
- [ ] No domain entity exposed
- [ ] Only references `BuildingBlocks.EventBus`
- [ ] Integration event is a record type

---

## Task 2.6: Users.Domain — Profile & Language Entities

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Users/EnglishTutor.Modules.Users.Domain/
├── Entities/
│   ├── UserProfile.cs
│   ├── UserLanguageSettings.cs
│   ├── UserTargetLanguage.cs
│   └── UserPreference.cs
├── Events/
│   ├── UserProfileCreatedDomainEvent.cs
│   ├── UserProfileUpdatedDomainEvent.cs
│   ├── UserLanguageSettingsUpdatedDomainEvent.cs
│   ├── UserTargetLanguageAddedDomainEvent.cs
│   └── UserLevelChangedDomainEvent.cs
├── ValueObjects/
│   └── DisplayName.cs
```

**UserProfile (AggregateRoot<Guid>):**
- `UserId (Guid)`, `DisplayName`, `AvatarUrl`, `Bio`, `CreatedAtUtc`, `UpdatedAtUtc`
- `static UserProfile Create(Guid userId, string displayName)`
- `void UpdateProfile(string displayName, string? avatarUrl, string? bio)`

**UserLanguageSettings (Entity<Guid>):**
- `UserId`, `NativeLanguageCode (LanguageCode)`, `UiLanguageCode`, `ExplanationLanguageCode`, `ActiveTargetLanguageCode`, `CreatedAtUtc`, `UpdatedAtUtc`
- `void Update(LanguageCode native, LanguageCode ui, LanguageCode explanation, LanguageCode activeTarget)`

**UserTargetLanguage (Entity<Guid>):**
- `UserId`, `TargetLanguageCode`, `CurrentLevel (LanguageLevel)`, `TargetLevel (LanguageLevel)`, `IsActive`, `CreatedAtUtc`, `UpdatedAtUtc`
- `void Activate()`, `void Deactivate()`
- `void UpdateLevel(LanguageLevel newLevel)` — raises `UserLevelChangedDomainEvent`

**Acceptance Criteria:**
- [ ] Uses SharedKernel `LanguageCode`, `LanguageLevel`
- [ ] Level changes tracked via domain events
- [ ] Only one target language active at a time (business rule)
- [ ] No infrastructure dependencies
- [ ] Domain does not reference Application `Result` or `Error`

---

## Task 2.7: Users.Application — Commands & Queries

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Users/EnglishTutor.Modules.Users.Application/
├── Commands/
│   ├── CreateUserProfile/
│   │   ├── CreateUserProfileCommand.cs
│   │   └── CreateUserProfileCommandHandler.cs
│   ├── UpdateUserProfile/
│   │   ├── UpdateUserProfileCommand.cs
│   │   ├── UpdateUserProfileCommandHandler.cs
│   │   └── UpdateUserProfileCommandValidator.cs
│   ├── UpdateLanguageSettings/
│   │   ├── UpdateLanguageSettingsCommand.cs
│   │   ├── UpdateLanguageSettingsCommandHandler.cs
│   │   └── UpdateLanguageSettingsCommandValidator.cs
│   ├── AddTargetLanguage/
│   │   ├── AddTargetLanguageCommand.cs
│   │   └── AddTargetLanguageCommandHandler.cs
│   └── ActivateTargetLanguage/
│       ├── ActivateTargetLanguageCommand.cs
│       └── ActivateTargetLanguageCommandHandler.cs
├── Queries/
│   ├── GetUserProfile/
│   │   ├── GetUserProfileQuery.cs
│   │   └── GetUserProfileQueryHandler.cs
│   ├── GetLanguageSettings/
│   │   ├── GetLanguageSettingsQuery.cs
│   │   └── GetLanguageSettingsQueryHandler.cs
│   └── GetTargetLanguages/
│       ├── GetTargetLanguagesQuery.cs
│       └── GetTargetLanguagesQueryHandler.cs
├── Abstractions/
│   ├── IUserProfileRepository.cs
│   ├── IUserLanguageSettingsRepository.cs
│   └── IUserTargetLanguageRepository.cs
├── EventHandlers/
│   └── UserRegisteredIntegrationEventHandler.cs
├── Errors/
│   └── UserErrors.cs
└── DTOs/
    ├── UserProfileResponse.cs
    ├── LanguageSettingsResponse.cs
    └── TargetLanguageResponse.cs
```

**UserErrors:**
- Lives in `Users.Application`, not `Users.Domain`
- Uses `EnglishTutor.BuildingBlocks.Application.Results.Error`
- Includes expected use-case failures such as `ProfileNotFound`, `LanguageSettingsNotFound`, `TargetLanguageNotFound`, `TargetLanguageAlreadyExists`, `CannotDeactivateActiveTargetLanguage`

**UserRegisteredIntegrationEventHandler:**
- Consumes `UserRegisteredIntegrationEvent` from Auth
- Creates `UserProfile` with default settings
- Creates default `UserLanguageSettings` (native=vi, ui=vi, explanation=vi, target=en)
- Creates initial `UserTargetLanguage` (en, A1, B2, active=true)
- Uses Inbox to ensure idempotency

**Acceptance Criteria:**
- [ ] User profile auto-created when Auth publishes registration event
- [ ] All handlers use Result pattern
- [ ] CancellationToken on all async methods
- [ ] Inbox idempotency in event handler

---

## Task 2.8: Users.Infrastructure — DbContext, Contracts Implementation

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Users/EnglishTutor.Modules.Users.Infrastructure/
├── Persistence/
│   ├── UsersDbContext.cs
│   ├── Configurations/
│   │   ├── UserProfileConfiguration.cs
│   │   ├── UserLanguageSettingsConfiguration.cs
│   │   ├── UserTargetLanguageConfiguration.cs
│   │   └── UserPreferenceConfiguration.cs
│   ├── Repositories/
│   │   ├── UserProfileRepository.cs
│   │   ├── UserLanguageSettingsRepository.cs
│   │   └── UserTargetLanguageRepository.cs
│   └── Migrations/
├── ContractReaders/
│   ├── UserProfileReader.cs
│   ├── UserLanguageSettingsReader.cs
│   └── UserTargetLanguageReader.cs
├── DependencyInjection.cs
└── OutboxIntegration/
    └── UsersDomainEventToOutboxMapper.cs
```

**UsersDbContext:**
- Schema: `"users"`
- MigrationsHistoryTable in `"users"` schema
- DbSets: `UserProfiles`, `UserLanguageSettings`, `UserTargetLanguages`, `UserPreferences`

**Contract Readers (implement interfaces from Users.Contracts):**
- `UserProfileReader : IUserProfileReader` — queries `UsersDbContext`, returns `UserProfileReadModel`
- `UserLanguageSettingsReader : IUserLanguageSettingsReader` — returns `UserLanguageSettingsReadModel`
- `UserTargetLanguageReader : IUserTargetLanguageReader` — returns `UserTargetLanguageReadModel`

**Key rule:** Contract readers return DTOs/read models only, never domain entities

**Acceptance Criteria:**
- [ ] Schema is `"users"`
- [ ] Contract readers return read models, not entities
- [ ] Indexes on `UserId` for all tables
- [ ] Unique index on `(UserId, TargetLanguageCode)` for UserTargetLanguages

---

## Task 2.9: Users.Presentation — API Endpoints

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Users/EnglishTutor.Modules.Users.Presentation/
├── UserEndpoints.cs
└── Requests/
    ├── UpdateProfileRequest.cs
    ├── UpdateLanguageSettingsRequest.cs
    ├── AddTargetLanguageRequest.cs
    └── ActivateTargetLanguageRequest.cs
```

**Endpoints:**

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/users/me/profile` | Yes | Get current user profile |
| PUT | `/api/users/me/profile` | Yes | Update profile |
| GET | `/api/users/me/language-settings` | Yes | Get language settings |
| PUT | `/api/users/me/language-settings` | Yes | Update language settings |
| GET | `/api/users/me/target-languages` | Yes | List target languages |
| POST | `/api/users/me/target-languages` | Yes | Add target language |
| PUT | `/api/users/me/target-languages/{id}/activate` | Yes | Activate target language |

**Acceptance Criteria:**
- [ ] All 7 endpoints require authentication
- [ ] `me` routes use `ICurrentUser.UserId`
- [ ] No business logic in endpoint methods
- [ ] Proper HTTP status codes (200, 201, 404, 422)

---

## Task 2.10: Users.Contracts

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Users/EnglishTutor.Modules.Users.Contracts/
├── Readers/
│   ├── IUserProfileReader.cs
│   ├── IUserLanguageSettingsReader.cs
│   └── IUserTargetLanguageReader.cs
├── ReadModels/
│   ├── UserProfileReadModel.cs
│   ├── UserLanguageSettingsReadModel.cs
│   └── UserTargetLanguageReadModel.cs
└── IntegrationEvents/
    ├── UserProfileUpdatedIntegrationEvent.cs
    ├── UserLanguageSettingsUpdatedIntegrationEvent.cs
    ├── UserTargetLanguageChangedIntegrationEvent.cs
    └── UserLevelChangedIntegrationEvent.cs
```

**UserLanguageSettingsReadModel:**
```csharp
public sealed record UserLanguageSettingsReadModel(
    Guid UserId,
    string NativeLanguageCode,
    string TargetLanguageCode,
    string UiLanguageCode,
    string ExplanationLanguageCode,
    string CurrentLevel,
    string TargetLevel);
```

**UserLevelChangedIntegrationEvent:**
- `Guid UserId`, `string TargetLanguageCode`, `string PreviousLevel`, `string NewLevel`, `DateTime ChangedAtUtc`

**Acceptance Criteria:**
- [ ] Only DTOs, read models, interfaces, and integration events
- [ ] No domain entities referenced
- [ ] Read model records are immutable
- [ ] All reader interfaces use `CancellationToken`

---

## Task 2.11: Users Outbox Integration

**Agent:** Integration & Events Agent

**Description:** Wire domain events → integration events → outbox in Users module.

**Implementation:**
1. After `UsersDbContext.SaveChangesAsync`, collect domain events from aggregates
2. Map each domain event to integration event (e.g., `UserProfileUpdatedDomainEvent` → `UserProfileUpdatedIntegrationEvent`)
3. Serialize integration event to JSON
4. Create `OutboxMessage` and save to `users.OutboxMessages` table
5. Add `OutboxMessage` EF configuration in `UsersDbContext`

**OutboxMessage table in users schema:**
```sql
users.OutboxMessages (
    Id, EventId, EventType, Payload, Status, RetryCount, MaxRetryCount,
    NextRetryAtUtc, LockedBy, LockedUntilUtc, CreatedAtUtc, ProcessedAtUtc, LastError
)
```

**Acceptance Criteria:**
- [ ] Domain events mapped to integration events automatically
- [ ] OutboxMessages saved in same transaction as business data
- [ ] Outbox table lives in `users` schema
- [ ] Worker can process users outbox messages

---

## Task 2.12: Auth + Users Unit Tests

**Agent:** Test Writer

**Files to create:**

```
tests/EnglishTutor.Modules.Auth.UnitTests/
├── Domain/
│   ├── AuthUserTests.cs
│   ├── EmailValueObjectTests.cs
│   └── RefreshTokenTests.cs
├── Application/
│   ├── RegisterCommandHandlerTests.cs
│   └── LoginCommandHandlerTests.cs
└── Validators/
    ├── RegisterCommandValidatorTests.cs
    └── LoginCommandValidatorTests.cs

tests/EnglishTutor.Modules.Users.UnitTests/
├── Domain/
│   ├── UserProfileTests.cs
│   ├── UserLanguageSettingsTests.cs
│   └── UserTargetLanguageTests.cs
└── Application/
    ├── CreateUserProfileCommandHandlerTests.cs
    └── UpdateLanguageSettingsCommandHandlerTests.cs
```

**Key test cases:**
- AuthUser.Register raises domain event
- Email rejects invalid format
- RefreshToken.Revoke sets RevokedAtUtc
- RegisterHandler fails on duplicate email
- LoginHandler fails on wrong password
- UserTargetLanguage.UpdateLevel raises domain event
- Only one target language can be active
- Validators reject invalid input

**Acceptance Criteria:**
- [ ] At least 20 unit tests across both modules
- [ ] Domain logic tested without mocking infrastructure
- [ ] Handler tests mock repositories
- [ ] All tests pass

---

## Task 2.13: Architecture Tests — Auth/Users Boundary Rules

**Agent:** Architecture Guardian

**Files to modify:**

```
tests/EnglishTutor.ArchitectureTests/ModuleBoundaryTests.cs
tests/EnglishTutor.ArchitectureTests/LayerDependencyTests.cs
```

**New rules:**
- Auth.Domain must not reference Users namespace
- Users.Domain must not reference Auth namespace
- Auth.Infrastructure must not be referenced by Users
- Users.Infrastructure must not be referenced by Auth
- Auth.Contracts must not expose `AuthUser` entity
- Users.Contracts must not expose `UserProfile` entity

**Acceptance Criteria:**
- [ ] Tests verify Auth↔Users isolation
- [ ] Tests fail if someone adds cross-module domain reference
- [ ] All architecture tests pass

---

## Task 2.14: API Documentation — Auth & Users

**Agent:** Documentation Agent

**Files to create:**

```
docs/api/auth.md
docs/api/users.md
```

**Each doc includes:**
- Endpoint, method, route
- Purpose
- Auth requirement
- Request body (JSON example)
- Response body (JSON example)
- Validation rules
- Error codes (with HTTP status)
- Application flow (step by step)
- Related modules
- Integration events produced
- Integration events consumed

**Acceptance Criteria:**
- [ ] All 12 endpoints documented (5 Auth + 7 Users)
- [ ] Request/response examples are valid JSON
- [ ] Validation rules match FluentValidation rules
- [ ] Error codes match Application `AuthErrors` and `UserErrors`

---

## Phase 2 Definition of Done

- [ ] Register → Login → JWT → access `/api/users/me/profile` works e2e
- [ ] Language settings CRUD works
- [ ] Target languages CRUD works
- [ ] UserRegistered event → auto-create UserProfile via Outbox/Worker/Inbox
- [ ] `dotnet build && dotnet test` passes
- [ ] Architecture tests verify Auth↔Users isolation
- [ ] `docs/api/auth.md` and `docs/api/users.md` created
- [ ] At least 20 unit tests pass
