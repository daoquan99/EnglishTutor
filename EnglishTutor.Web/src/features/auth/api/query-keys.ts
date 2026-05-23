export const authKeys = {
  all: ["auth"] as const,
  me: () => [...authKeys.all, "me"] as const,
};

/**
 * Query keys for the Auth admin surface (roles, permissions, users).
 * Mirrors the backend module so that pages under `features/auth/components/admin/`
 * keep their data scoped to this module's cache namespace.
 */
export const authAdminKeys = {
  all: ["auth", "admin"] as const,
  permissions: {
    all: ["auth", "admin", "permissions"] as const,
    list: () => [...authAdminKeys.permissions.all, "list"] as const,
  },
  roles: {
    all: ["auth", "admin", "roles"] as const,
    list: () => [...authAdminKeys.roles.all, "list"] as const,
    detail: (id: string) => [...authAdminKeys.roles.all, "detail", id] as const,
  },
  users: {
    all: ["auth", "admin", "users"] as const,
    list: (params?: Record<string, unknown>) =>
      [...authAdminKeys.users.all, "list", params] as const,
    detail: (userId: string) =>
      [...authAdminKeys.users.all, "detail", userId] as const,
  },
};
