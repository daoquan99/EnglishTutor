"use client";

import { useState } from "react";
import { useCurrentUser } from "@/features/auth/hooks/use-current-user";
import { ADMIN_AREA_PERMISSIONS } from "@/features/auth/lib/permission-codes";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { useAdminPermission } from "@/shared/admin";
import { AdminSidebar } from "@/features/admin-reports/components/admin-sidebar";
import { AdminHeader } from "@/features/admin-reports/components/admin-header";

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  const { isPending, isSuccess } = useCurrentUser();
  const { hasAny } = useAdminPermission();
  const [sidebarOpen, setSidebarOpen] = useState(false);

  if (isPending) {
    return (
      <div className="flex min-h-dvh items-center justify-center">
        <div className="grid gap-3 text-center">
          <Skeleton className="mx-auto h-10 w-10 rounded-full" />
          <Skeleton className="h-4 w-32" />
        </div>
      </div>
    );
  }

  if (!isSuccess || !hasAny(ADMIN_AREA_PERMISSIONS)) {
    return (
      <div className="flex min-h-dvh items-center justify-center">
        <div className="text-center">
          <div className="mx-auto mb-4 flex size-16 items-center justify-center rounded-full bg-destructive/10">
            <span className="text-2xl font-bold text-destructive">403</span>
          </div>
          <h1 className="text-lg font-semibold">Access Denied</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Bạn không có quyền truy cập khu vực admin.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex min-h-dvh">
      <AdminSidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} />
      <div className="flex flex-1 flex-col overflow-hidden">
        <AdminHeader onMenuClick={() => setSidebarOpen(true)} />
        <main className="flex-1 overflow-y-auto">{children}</main>
      </div>
    </div>
  );
}
