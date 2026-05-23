"use client";

import { useMemo } from "react";
import { useAuthStore } from "@/features/auth/hooks/use-auth-store";

export const ADMIN_WILDCARD_PERMISSION = "admin.full_access";

export type UseAdminPermissionResult = {
  permissions: ReadonlySet<string>;
  isAdmin: boolean;
  has: (code: string) => boolean;
  hasAny: (codes: readonly string[] | undefined) => boolean;
  hasAll: (codes: readonly string[] | undefined) => boolean;
};

export function useAdminPermission(): UseAdminPermissionResult {
  const userPermissions = useAuthStore((state) => state.user?.permissions);

  return useMemo<UseAdminPermissionResult>(() => {
    const set = new Set(userPermissions ?? []);
    const isAdmin = set.has(ADMIN_WILDCARD_PERMISSION);

    const has = (code: string) => isAdmin || set.has(code);

    const hasAny = (codes: readonly string[] | undefined) => {
      if (!codes || codes.length === 0) {
        return true;
      }
      if (isAdmin) {
        return true;
      }
      return codes.some((code) => set.has(code));
    };

    const hasAll = (codes: readonly string[] | undefined) => {
      if (!codes || codes.length === 0) {
        return true;
      }
      if (isAdmin) {
        return true;
      }
      return codes.every((code) => set.has(code));
    };

    return { permissions: set, isAdmin, has, hasAny, hasAll };
  }, [userPermissions]);
}
