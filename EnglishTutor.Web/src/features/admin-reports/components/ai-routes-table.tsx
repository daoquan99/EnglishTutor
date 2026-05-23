"use client";

import { useMemo } from "react";
import { Badge } from "@/shared/components/ui/badge";
import { AdminDataTable, type AdminDataTableColumn } from "@/shared/admin";
import type { AiRuntimeRoute } from "../types/admin-reports";

export function AiRoutesTable({ routes }: { routes?: AiRuntimeRoute[] }) {
  const columns: AdminDataTableColumn<AiRuntimeRoute>[] = useMemo(
    () => [
      {
        id: "taskType",
        header: "Task Type",
        cell: (r) => <span className="font-medium">{r.taskType}</span>,
      },
      { id: "capability", header: "Capability", cell: (r) => r.capability },
      {
        id: "preferred",
        header: "Preferred",
        cell: (r) => (
          <span className="text-xs">
            {r.preferredProviderName}/{r.preferredModelCode}
          </span>
        ),
      },
      {
        id: "fallback",
        header: "Fallback",
        cell: (r) => (
          <span className="text-xs text-muted-foreground">
            {r.fallbackProviderName
              ? `${r.fallbackProviderName}/${r.fallbackModelCode}`
              : "None"}
          </span>
        ),
      },
      {
        id: "status",
        header: "Status",
        cell: (r) => (
          <Badge variant={r.isActive ? "default" : "secondary"}>
            {r.isActive ? "Active" : "Inactive"}
          </Badge>
        ),
      },
    ],
    [],
  );

  return (
    <AdminDataTable<AiRuntimeRoute>
      data={routes}
      isLoading={!routes}
      getRowId={(r) => r.id}
      emptyMessage="Chưa có AI route nào."
      columns={columns}
    />
  );
}
