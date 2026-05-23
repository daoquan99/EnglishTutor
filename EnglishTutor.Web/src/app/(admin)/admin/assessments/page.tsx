"use client";

import { BookCheck } from "lucide-react";
import { useMemo } from "react";
import { PageHeader } from "@/shared/components/page-header";
import {
  AdminAccessDenied,
  AdminDataTable,
  AdminPermissionGate,
  type AdminDataTableColumn,
} from "@/shared/admin";
import { PermissionCodes } from "@/features/auth/lib/permission-codes";
import { useAssessmentPassRates } from "@/features/admin-reports/hooks/use-admin-reports";
import type { AssessmentPassRateReport } from "@/features/admin-reports/types/admin-reports";

export default function AdminAssessmentsPage() {
  const { data, isFetching } = useAssessmentPassRates();

  const columns: AdminDataTableColumn<AssessmentPassRateReport>[] = useMemo(
    () => [
      { id: "type", header: "Type", cell: (r) => r.assessmentType },
      { id: "level", header: "Level", cell: (r) => r.forLevel },
      {
        id: "attempts",
        header: "Attempts",
        align: "right",
        cell: (r) => <span className="tabular-nums">{r.totalAttempts}</span>,
      },
      {
        id: "passRate",
        header: "Pass Rate",
        align: "right",
        cell: (r) => (
          <span className="tabular-nums">{(r.passRate * 100).toFixed(1)}%</span>
        ),
      },
      {
        id: "avgScore",
        header: "Avg Score",
        align: "right",
        cell: (r) => (
          <span className="tabular-nums">{r.averageScore.toFixed(1)}</span>
        ),
      },
    ],
    [],
  );

  return (
    <AdminPermissionGate
      requireAny={[
        PermissionCodes.AssessmentsRead,
        PermissionCodes.ReportsRead,
      ]}
      fallback={<AdminAccessDenied />}
    >
      <div className="page-container page-section">
        <PageHeader
          icon={BookCheck}
          iconColor="bg-vocabulary/10 text-vocabulary"
          title="Assessment Pass Rates"
          description="Assessment completion and scoring statistics."
        />
        <div className="mt-6">
          <AdminDataTable<AssessmentPassRateReport>
            data={data}
            isLoading={!data}
            isFetching={isFetching}
            getRowId={(r) =>
              `${r.targetLanguageCode}-${r.assessmentType}-${r.forLevel}`
            }
            emptyMessage="Chưa có dữ liệu assessment."
            columns={columns}
          />
        </div>
      </div>
    </AdminPermissionGate>
  );
}
