---
name: admin-feature
description: Implements an admin panel feature — permission-gated page with data tables, CRUD dialogs, search/pagination, and admin-specific patterns. Uses the shared admin primitives in src/shared/admin/.
---

# Admin Feature

## When to invoke

- New admin page (e.g., content management, user management, AI usage dashboard).
- Adding CRUD operations to an existing admin page.
- Adding permission-gated admin functionality.

## Inputs

- Feature name and admin route (e.g., `admin/users`, `admin/ai-providers`).
- Backend endpoints being consumed.
- Required permission code(s) from `Auth.Contracts`.

## Shared admin primitives

```
src/shared/admin/
├── components/
│   ├── admin-access-denied.tsx       # fallback when permission denied
│   ├── admin-crud-dialog.tsx         # reusable create/edit dialog wrapper
│   ├── admin-data-table.tsx          # paginated data table with search
│   ├── admin-delete-confirm-dialog.tsx
│   ├── admin-empty-state.tsx
│   ├── admin-page-header.tsx
│   └── admin-permission-gate.tsx     # renders children only if user has permission
├── hooks/
│   ├── use-admin-permission.ts       # permission checking hook
│   └── use-admin-toast.ts            # admin-specific toast helpers
└── index.ts
```

Use these primitives. Don't fork them for a single feature — compose them.

## Step-by-step

### 1. Define types

`src/features/{feature}/types/{feature}.ts`:

```ts
export interface UserOverviewDto {
  id: string
  email: string
  displayName: string
  isDeleted: boolean
  createdAtUtc: string
  // ... match backend DTO exactly, including Utc suffix
}
```

### 2. Add API functions

`src/features/{feature}/api/admin-{feature}-api.ts`:

```ts
import { httpClient } from '@/shared/api/http-client'
import type { PagedResponse } from '@/shared/types/pagination'
import type { UserOverviewDto } from '../types/{feature}'

export const adminUsersApi = {
  list: (params: { page: number; pageSize: number; search?: string }) =>
    httpClient.get<PagedResponse<UserOverviewDto>>('/admin/users', { params }),
  getById: (id: string) =>
    httpClient.get<UserOverviewDto>(`/admin/users/${id}`),
  delete: (id: string) =>
    httpClient.delete(`/admin/users/${id}`),
}
```

### 3. Add query keys

`src/features/{feature}/api/query-keys.ts`:

```ts
export const adminUsersKeys = {
  all: ['admin', 'users'] as const,
  list: (params: Record<string, unknown>) => [...adminUsersKeys.all, 'list', params] as const,
  detail: (id: string) => [...adminUsersKeys.all, 'detail', id] as const,
}
```

### 4. Add hooks

`src/features/{feature}/hooks/use-admin-{feature}.ts`:

```ts
export function useAdminUsers(params: { page: number; pageSize: number; search?: string }) {
  return useQuery({
    queryKey: adminUsersKeys.list(params),
    queryFn: () => adminUsersApi.list(params),
  })
}
```

Mutations should invalidate the list query on success:

```ts
export function useDeleteUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => adminUsersApi.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: adminUsersKeys.all }),
  })
}
```

### 5. Build the table component

`src/features/{feature}/components/{feature}-table.tsx`:

```tsx
import { AdminDataTable, type AdminDataTableColumn } from '@/shared/admin/components/admin-data-table'
import { AdminPermissionGate } from '@/shared/admin/components/admin-permission-gate'

const columns: AdminDataTableColumn<UserOverviewDto>[] = [
  { id: 'email', header: 'Email', cell: (row) => row.email },
  { id: 'name', header: 'Tên', cell: (row) => row.displayName },
  // ...
]

export function UsersTable() {
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)
  const [search, setSearch] = useState('')
  const { data, isLoading, isFetching } = useAdminUsers({ page, pageSize, search })

  return (
    <AdminPermissionGate requireAny={['auth.users.read']}>
      <AdminDataTable
        columns={columns}
        data={data?.items}
        isLoading={isLoading}
        isFetching={isFetching}
        getRowId={(row) => row.id}
        search={{ value: search, onChange: setSearch, placeholder: 'Tìm user...' }}
        pagination={{
          page,
          pageSize,
          total: data?.totalCount ?? 0,
          onPageChange: setPage,
          onPageSizeChange: setPageSize,
        }}
      />
    </AdminPermissionGate>
  )
}
```

### 6. Add CRUD dialogs (if applicable)

Use `AdminCrudDialog` for create/edit forms and `AdminDeleteConfirmDialog` for deletions. Wire form state with `react-hook-form` + Zod.

### 7. Wire the route

`src/app/(admin)/admin/{feature}/page.tsx`:

```tsx
import { AdminPageHeader } from '@/shared/admin/components/admin-page-header'
import { UsersTable } from '@/features/auth/components/admin/users-table'

export default function Page() {
  return (
    <>
      <AdminPageHeader title="Quản lý người dùng" />
      <UsersTable />
    </>
  )
}
```

Admin routes live in `src/app/(admin)/admin/`. Keep the page file thin.

### 8. Permission gating

- Use `AdminPermissionGate` to conditionally render UI sections.
- Use `useAdminPermission().has('code')` for conditional logic in hooks/handlers.
- Permission codes come from `Auth.Contracts` on backend and `features/auth/lib/permission-codes.ts` on frontend.
- `admin.full_access` is the wildcard — holders pass all checks.

### 9. Sidebar entry

Add the new page to `src/features/admin-reports/components/admin-sidebar.tsx` with the appropriate icon and permission gate.

## Forbidden

- Hardcoded role checks (e.g., `if (role === 'Admin')`) — use permission codes.
- Creating a separate admin layout outside `(admin)` route group.
- Putting business logic in the page file.
- Skipping loading/empty/error states in the table.
- Forking `AdminDataTable` or `AdminCrudDialog` for one-off needs — compose them.

## Commands

```bash
cd EnglishTutor.Web
pnpm dev
pnpm typecheck
pnpm lint
```

## Done when

- Admin page renders with proper data table (loading, empty, populated states).
- Permission gate blocks unauthorized users with a clear fallback.
- CRUD operations invalidate related queries.
- Sidebar entry added.
- `pnpm typecheck` + `pnpm lint` pass.
- No role-based checks — permission codes only.
