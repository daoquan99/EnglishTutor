"use client";

import { useMemo } from "react";
import { AdminDataTable, type AdminDataTableColumn } from "@/shared/admin";
import type { AuditLog } from "../types/admin-reports";

function formatDate(utc: string) {
  return new Date(utc).toLocaleString(undefined, {
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function AuditLogTable({ logs }: { logs?: AuditLog[] }) {
  const columns: AdminDataTableColumn<AuditLog>[] = useMemo(
    () => [
      {
        id: "action",
        header: "Action",
        cell: (log) => <span className="font-medium">{log.action}</span>,
      },
      { id: "target", header: "Target", cell: (log) => log.targetEntity },
      {
        id: "entityId",
        header: "Entity ID",
        cell: (log) => (
          <span className="font-mono text-xs">
            {log.targetEntityId.slice(0, 8)}...
          </span>
        ),
      },
      {
        id: "date",
        header: "Date",
        cell: (log) => (
          <span className="text-xs">{formatDate(log.createdAtUtc)}</span>
        ),
      },
    ],
    [],
  );

  return (
    <AdminDataTable<AuditLog>
      data={logs}
      isLoading={!logs}
      getRowId={(log) => log.id}
      emptyMessage="Chưa có audit log."
      columns={columns}
    />
  );
}
