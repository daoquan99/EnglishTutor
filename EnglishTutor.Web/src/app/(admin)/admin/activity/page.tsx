"use client";

import { Activity } from "lucide-react";
import { useMemo } from "react";
import { PageHeader } from "@/shared/components/page-header";
import {
  AdminAccessDenied,
  AdminDataTable,
  AdminPermissionGate,
  type AdminDataTableColumn,
} from "@/shared/admin";
import { PermissionCodes } from "@/features/auth/lib/permission-codes";
import { useLearningActivityReport } from "@/features/admin-reports/hooks/use-admin-reports";
import type { LearningActivityReport } from "@/features/admin-reports/types/admin-reports";

export default function LearningActivityPage() {
  const { data, isFetching } = useLearningActivityReport();

  const columns: AdminDataTableColumn<LearningActivityReport>[] = useMemo(
    () => [
      { id: "date", header: "Date", cell: (r) => r.reportDate },
      {
        id: "users",
        header: "Active Users",
        align: "right",
        cell: (r) => <span className="tabular-nums">{r.totalActiveUsers}</span>,
      },
      {
        id: "speaking",
        header: "Speaking",
        align: "right",
        cell: (r) => (
          <span className="tabular-nums">{r.totalSpeakingSessions}</span>
        ),
      },
      {
        id: "exercises",
        header: "Exercises",
        align: "right",
        cell: (r) => (
          <span className="tabular-nums">{r.totalExercisesCompleted}</span>
        ),
      },
      {
        id: "vocab",
        header: "Vocabulary",
        align: "right",
        cell: (r) => (
          <span className="tabular-nums">{r.totalVocabularyReviews}</span>
        ),
      },
      {
        id: "minutes",
        header: "Study (min)",
        align: "right",
        cell: (r) => <span className="tabular-nums">{r.totalStudyMinutes}</span>,
      },
    ],
    [],
  );

  return (
    <AdminPermissionGate
      requireAny={[PermissionCodes.ReportsRead]}
      fallback={<AdminAccessDenied />}
    >
      <div className="page-container page-section">
        <PageHeader
          icon={Activity}
          iconColor="bg-study/10 text-study"
          title="Learning Activity"
          description="Daily learning activity reports across all users."
        />
        <div className="mt-6">
          <AdminDataTable<LearningActivityReport>
            data={data}
            isLoading={!data}
            isFetching={isFetching}
            getRowId={(r) => `${r.reportDate}-${r.period}`}
            emptyMessage="Chưa có dữ liệu hoạt động học tập."
            columns={columns}
          />
        </div>
      </div>
    </AdminPermissionGate>
  );
}
