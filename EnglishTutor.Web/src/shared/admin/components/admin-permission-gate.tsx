"use client";

import type { ReactNode } from "react";
import { useAdminPermission } from "@/shared/admin/hooks/use-admin-permission";

type AdminPermissionGateProps = {
  requireAny?: readonly string[];
  requireAll?: readonly string[];
  fallback?: ReactNode;
  children: ReactNode;
};

export function AdminPermissionGate({
  requireAny,
  requireAll,
  fallback = null,
  children,
}: AdminPermissionGateProps) {
  const { hasAny, hasAll } = useAdminPermission();

  const allowed =
    (!requireAny || hasAny(requireAny)) && (!requireAll || hasAll(requireAll));

  if (!allowed) {
    return <>{fallback}</>;
  }
  return <>{children}</>;
}
