"use client";

import { useMemo } from "react";
import { Badge } from "@/shared/components/ui/badge";
import { AdminDataTable, type AdminDataTableColumn } from "@/shared/admin";
import type { AiProvider } from "../types/admin-reports";

export function AiProvidersTable({
  providers,
}: {
  providers?: AiProvider[];
}) {
  const columns: AdminDataTableColumn<AiProvider>[] = useMemo(
    () => [
      {
        id: "provider",
        header: "Provider",
        cell: (p) => (
          <div>
            <p className="font-medium">{p.displayName}</p>
            <p className="text-xs text-muted-foreground">{p.providerName}</p>
          </div>
        ),
      },
      { id: "type", header: "Type", cell: (p) => p.providerType },
      {
        id: "models",
        header: "Models",
        cell: (p) => (
          <span className="text-xs text-muted-foreground">
            {p.models.length} models
          </span>
        ),
      },
      {
        id: "status",
        header: "Status",
        cell: (p) => (
          <Badge variant={p.isEnabled ? "default" : "secondary"}>
            {p.isEnabled ? "Active" : "Disabled"}
          </Badge>
        ),
      },
    ],
    [],
  );

  return (
    <AdminDataTable<AiProvider>
      data={providers}
      isLoading={!providers}
      getRowId={(p) => p.id}
      emptyMessage="Chưa có AI provider nào."
      columns={columns}
    />
  );
}
