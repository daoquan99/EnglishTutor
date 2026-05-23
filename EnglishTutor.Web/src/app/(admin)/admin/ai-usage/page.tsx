"use client";

import { Brain } from "lucide-react";
import { useMemo } from "react";
import { PageHeader } from "@/shared/components/page-header";
import {
  AdminAccessDenied,
  AdminDataTable,
  AdminPermissionGate,
  type AdminDataTableColumn,
} from "@/shared/admin";
import { PermissionCodes } from "@/features/auth/lib/permission-codes";
import { useAiUsageReport } from "@/features/admin-reports/hooks/use-admin-reports";
import type { DailyAiUsageReport } from "@/features/admin-reports/types/admin-reports";

export default function AiUsagePage() {
  const { data, isFetching } = useAiUsageReport();

  const columns: AdminDataTableColumn<DailyAiUsageReport>[] = useMemo(
    () => [
      { id: "date", header: "Date", cell: (r) => r.reportDate },
      { id: "model", header: "Model", cell: (r) => r.modelType },
      { id: "task", header: "Task", cell: (r) => r.taskType },
      {
        id: "requests",
        header: "Requests",
        align: "right",
        cell: (r) => <span className="tabular-nums">{r.totalRequests}</span>,
      },
      {
        id: "tokens",
        header: "Tokens",
        align: "right",
        cell: (r) => (
          <span className="tabular-nums">{r.totalTokens.toLocaleString()}</span>
        ),
      },
      {
        id: "cost",
        header: "Cost (USD)",
        align: "right",
        cell: (r) => (
          <span className="tabular-nums">${r.estimatedCostUsd.toFixed(2)}</span>
        ),
      },
    ],
    [],
  );

  return (
    <AdminPermissionGate
      requireAny={[PermissionCodes.AiLogsRead, PermissionCodes.AiProvidersRead]}
      fallback={<AdminAccessDenied />}
    >
      <div className="page-container page-section">
        <PageHeader
          icon={Brain}
          iconColor="bg-speaking/10 text-speaking"
          title="AI Usage"
          description="Token usage, latency, and estimated costs."
        />
        <div className="mt-6">
          <AdminDataTable<DailyAiUsageReport>
            data={data}
            isLoading={!data}
            isFetching={isFetching}
            getRowId={(r) => `${r.reportDate}-${r.modelType}-${r.taskType}`}
            emptyMessage="Chưa có dữ liệu sử dụng AI."
            columns={columns}
          />
        </div>
      </div>
    </AdminPermissionGate>
  );
}
