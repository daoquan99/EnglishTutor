# Auth module

Identity provider for the platform. Owns users-as-identities (not profiles), credentials, sessions, roles, permissions, and security audit events.

## Schema

`auth`

## Aggregate roots

| Aggregate              | Purpose                                                                 |
| ---------------------- | ----------------------------------------------------------------------- |
| `AuthUser`             | Identity record: email, hashed password (BCrypt), email-verification. Owns role assignments (`AuthUserRole` child entity in `AuthUser/Entities/`). Methods: `UpdateDisplayName`, `Suspend`, `Restore`, `AssignRole`, `RemoveRole`, `SetRoles`. |
| `AuthSession`          | Active refresh-token / device session.                                  |
| `AuthRole`             | Named role, container of permissions. Owns `AuthRolePermission` child entity. |
| `AuthPermission`       | Permission code definition (lookup table seeded at startup).            |
| `AuthSecurityEvent`    | Audit log: login, logout, failed login, password change, lockout, admin-initiated user changes (suspend/restore/update/roles). |

## Contracts surface

- `Permissions/` — permission code constants (`Permissions.Ai.ProvidersManage`, `Permissions.Vocabulary.MasteryWrite`, ...). Other modules reference these.
- `ReadModels/` — DTOs for shared identity views.
- `IntegrationEvents/`:
  - `UserRegisteredIntegrationEvent` — published on successful registration. Consumed by Users (creates profile), Notifications (welcome email), Progress (initial EXP record).

## Key behaviors

- JWT issuance with permission claims (resolved at login from role assignments + wildcard).
- BCrypt password hashing.
- Session refresh / revocation flow.
- Lockout policy after N failed logins (configurable).
- Seeded admin user: `admin@englishtutor.local` with `admin.full_access` permission.
- **Admin user management endpoints** (`/api/admin/auth/users/...`) for list, detail, update display name, suspend/restore, and bulk role assignment. Read endpoints gated by `auth.users.read`; write endpoints gated by `auth.users.manage`. All admin write operations emit an `AuthSecurityEvent` row (event types `AdminUserUpdated`, `AdminUserSuspended`, `AdminUserRestored`, `AdminUserRolesChanged`).

## Notes for changes

- Permission code additions: add the constant in `Auth.Contracts/Permissions/` and add a description in `AuthDataSeeder.PermissionDescriptions`. Document in `docs/api/auth.md` when that file exists.
- Token format / claims change is a breaking change — coordinate with Web (`shared/api/auth-token-store.ts`).
- Don't gate at runtime on role names; use permission codes. See `../rules/authorization.md`.
- `AuthUserRole` lives under `AuthUser/Entities/` — never query it directly across modules; access role assignments through `AuthUser.Roles` or via `IAuthRolePermissionRepository.IsRoleAssignedToAnyUserAsync`.
