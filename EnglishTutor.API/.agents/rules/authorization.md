# Authorization rules

## Permission-based, not role-based

- All runtime authorization decisions check **permission codes**.
- Roles are containers of permissions — convenient grouping for seeding/admin UI, not the unit of access control.
- Never gate on a role name like `"Admin"` at runtime. Convert to a permission check.

## Permission constants

- Defined in `EnglishTutor.Modules.Auth.Contracts` so other modules may reference them through the allowed Contracts boundary.
- Format: lower-case dotted, `area.resource.action` (`ai.providers.manage`, `users.profile.read`, `vocabulary.mastery.write`, `admin.users.suspend`).

## Wildcard

- `admin.full_access` is the wildcard permission. It bypasses individual permission checks.
- Assigned only to the seeded full-admin role / account.
- Don't introduce other wildcards. Add specific permission codes instead.

## Enforcement points

- API endpoints: use the project's authorization filter / endpoint extension (e.g., `.RequirePermission(Permissions.Vocabulary.MasteryWrite)`).
- Application handlers may re-check permissions when an operation has finer-grained authorization than the endpoint (e.g., owner-only edits).
- Authorization checks must include the wildcard short-circuit so admins pass.

## Seeding

- Roles, permissions, and the initial admin user are seeded on startup when `SeedData__Enabled=true`.
- Admin password is configured via `SeedData__Admin__Password` (env, user-secrets, Aspire parameter, or secret manager). Not committed.
- Default admin email: `admin@englishtutor.local`.

## Forbidden

- Checking `User.IsInRole("...")` for runtime gating.
- Hard-coding permission strings outside `Auth.Contracts`.
- Skipping permission checks in admin endpoints because "admin should be allowed" — admins satisfy via `admin.full_access`, not by absence of a check.
- Adding role-name-based middleware.
