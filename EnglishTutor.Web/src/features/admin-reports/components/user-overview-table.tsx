"use client";

import { useMemo } from "react";
import { AdminDataTable, type AdminDataTableColumn } from "@/shared/admin";
import type { UserOverviewCard } from "../types/admin-reports";

function formatDate(utc: string | null) {
  if (!utc) return "Never";
  return new Date(utc).toLocaleDateString(undefined, {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

export function UserOverviewTable({ users }: { users?: UserOverviewCard[] }) {
  const columns: AdminDataTableColumn<UserOverviewCard>[] = useMemo(
    () => [
      {
        id: "user",
        header: "User",
        cell: (u) => (
          <div>
            <p className="font-medium">{u.displayName}</p>
            <p className="text-xs text-muted-foreground">{u.email}</p>
          </div>
        ),
      },
      { id: "level", header: "Level", cell: (u) => u.currentLevel },
      {
        id: "exp",
        header: "EXP",
        align: "right",
        cell: (u) => <span className="tabular-nums">{u.totalExp}</span>,
      },
      {
        id: "streak",
        header: "Streak",
        align: "right",
        cell: (u) => (
          <span className="tabular-nums">{u.currentStreakDays}d</span>
        ),
      },
      {
        id: "lastActive",
        header: "Last Active",
        cell: (u) => (
          <span className="text-xs">{formatDate(u.lastActivityAtUtc)}</span>
        ),
      },
    ],
    [],
  );

  return (
    <AdminDataTable<UserOverviewCard>
      data={users}
      isLoading={!users}
      getRowId={(u) => u.userId}
      emptyMessage="Chưa có user nào."
      columns={columns}
    />
  );
}
