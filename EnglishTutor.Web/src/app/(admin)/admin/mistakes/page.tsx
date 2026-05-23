"use client";

import { AlertTriangle } from "lucide-react";
import { useMemo } from "react";
import { PageHeader } from "@/shared/components/page-header";
import {
  AdminAccessDenied,
  AdminDataTable,
  AdminPermissionGate,
  type AdminDataTableColumn,
} from "@/shared/admin";
import { PermissionCodes } from "@/features/auth/lib/permission-codes";
import { useCommonMistakesReport } from "@/features/admin-reports/hooks/use-admin-reports";
import type { CommonMistakeStat } from "@/features/admin-reports/types/admin-reports";

export default function AdminMistakesPage() {
  const { data, isFetching } = useCommonMistakesReport();

  const columns: AdminDataTableColumn<CommonMistakeStat>[] = useMemo(
    () => [
      { id: "type", header: "Type", cell: (m) => m.mistakeType },
      { id: "category", header: "Category", cell: (m) => m.category },
      {
        id: "occurrences",
        header: "Occurrences",
        align: "right",
        cell: (m) => <span className="tabular-nums">{m.occurrenceCount}</span>,
      },
      {
        id: "users",
        header: "Users",
        align: "right",
        cell: (m) => <span className="tabular-nums">{m.affectedUsers}</span>,
      },
      {
        id: "example",
        header: "Example",
        cell: (m) => (
          <span className="text-xs">
            <span className="text-destructive line-through">
              {m.exampleOriginal}
            </span>{" "}
            <span className="text-success">{m.exampleCorrected}</span>
          </span>
        ),
      },
    ],
    [],
  );

  return (
    <AdminPermissionGate
      requireAny={[PermissionCodes.MistakesRead, PermissionCodes.ReportsRead]}
      fallback={<AdminAccessDenied />}
    >
      <div className="page-container page-section">
        <PageHeader
          icon={AlertTriangle}
          iconColor="bg-destructive/10 text-destructive"
          title="Common Mistakes"
          description="Most frequent mistake patterns across all users."
        />
        <div className="mt-6">
          <AdminDataTable<CommonMistakeStat>
            data={data}
            isLoading={!data}
            isFetching={isFetching}
            getRowId={(m) =>
              `${m.targetLanguageCode}-${m.mistakeType}-${m.category}`
            }
            emptyMessage="Chưa có thống kê lỗi."
            columns={columns}
          />
        </div>
      </div>
    </AdminPermissionGate>
  );
}
