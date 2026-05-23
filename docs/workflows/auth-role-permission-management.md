# Auth Role and Permission Management Workflow

## Overview

Admins manage Auth permissions and custom roles through permission-protected APIs. Runtime access decisions still use permission claims, not role names.

## Main Flow

1. Admin lists permissions or roles.
2. Admin creates or updates a custom permission when a new feature permission is needed.
3. Admin creates or updates a custom role by assigning enabled permission ids.
4. Auth synchronizes role-permission assignments in the `auth.RolePermissions` table.
5. Users receive updated permissions on the next login or refresh-issued access token.

## Detailed Steps

- Permission read requires `auth.permissions.read`.
- Permission create/update/delete requires `auth.permissions.manage`.
- Role read requires `auth.roles.read`.
- Role create/update/delete requires `auth.roles.manage`.
- `admin.full_access` remains a wildcard permission.
- System permissions from `PermissionCodes.All` cannot be deleted or disabled.
- System permissions cover Auth, Users, StudyPlans, LearningContent, Vocabulary, Exercises, Speaking, Mistakes, Assessments, Progress, Notifications, AI, and Reports permission groups.
- System roles such as seeded `admin` cannot be modified or deleted through admin CRUD.
- A permission assigned to a role cannot be deleted.
- A role assigned to a user cannot be deleted.

## Modules Involved

- Auth owns permissions, roles, role-permission assignments, and user-role assignments.
- Other modules reference only permission code constants from `Auth.Contracts`.

## Contracts Used

- `PermissionCodes` from `EnglishTutor.Modules.Auth.Contracts`.

## Events Published/Consumed

- None. Role/permission CRUD is local to Auth.

## Read Models/Projections Updated

- None.

## Failure/Retry Behavior

- Expected failures return `Result` errors through the shared API envelope.
- There is no Outbox/Inbox behavior for this workflow because it does not publish integration events.
