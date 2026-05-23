"use client";

import { useState } from "react";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { useCurrentUser } from "@/features/auth/hooks/use-current-user";
import { AppSidebar } from "@/shared/components/app-sidebar";
import { AppHeader } from "@/shared/components/app-header";
import { NotificationProvider } from "@/features/notifications/components/notification-provider";

export default function AppLayout({ children }: { children: React.ReactNode }) {
  const { isPending, isSuccess } = useCurrentUser();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);

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

  if (!isSuccess) {
    return null;
  }

  return (
    <div className="flex min-h-dvh">
      <NotificationProvider />
      <AppSidebar
        open={sidebarOpen}
        collapsed={sidebarCollapsed}
        onClose={() => setSidebarOpen(false)}
        onToggleCollapse={() => setSidebarCollapsed((prev) => !prev)}
      />
      <div className="flex flex-1 flex-col overflow-hidden">
        <AppHeader
          onMenuClick={() => setSidebarOpen(true)}
          collapsed={sidebarCollapsed}
          onToggleCollapse={() => setSidebarCollapsed((prev) => !prev)}
        />
        <main className="flex-1 overflow-y-auto">{children}</main>
      </div>
    </div>
  );
}
