"use client";

import { Route } from "lucide-react";
import { PageHeader } from "@/shared/components/page-header";
import { AdminAccessDenied, AdminPermissionGate } from "@/shared/admin";
import { PermissionCodes } from "@/features/auth/lib/permission-codes";
import { useAiRoutes } from "@/features/admin-reports/hooks/use-admin-ai";
import { AiRoutesTable } from "@/features/admin-reports/components/ai-routes-table";

export default function AiRoutesPage() {
  const { data } = useAiRoutes();

  return (
    <AdminPermissionGate
      requireAny={[PermissionCodes.AiRoutesRead, PermissionCodes.AiRoutesManage]}
      fallback={<AdminAccessDenied />}
    >
      <div className="page-container page-section">
        <PageHeader
          icon={Route}
          iconColor="bg-exercise/10 text-exercise"
          title="AI Routes"
          description="Configure which AI model handles each task type."
        />
        <div className="mt-6">
          <AiRoutesTable routes={data} />
        </div>
      </div>
    </AdminPermissionGate>
  );
}
