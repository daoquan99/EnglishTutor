"use client";

import { FileText } from "lucide-react";
import { PageHeader } from "@/shared/components/page-header";
import { AdminAccessDenied, AdminPermissionGate } from "@/shared/admin";
import { PermissionCodes } from "@/features/auth/lib/permission-codes";
import { useAuditLogs } from "@/features/admin-reports/hooks/use-admin-reports";
import { AuditLogTable } from "@/features/admin-reports/components/audit-log-table";

export default function AuditLogsPage() {
  const { data } = useAuditLogs();

  return (
    <AdminPermissionGate
      requireAny={[PermissionCodes.AuthSecurityEventsRead]}
      fallback={<AdminAccessDenied />}
    >
      <div className="page-container page-section">
        <PageHeader
          icon={FileText}
          title="Audit Logs"
          description="Track administrative actions and changes."
        />
        <div className="mt-6">
          <AuditLogTable logs={data} />
        </div>
      </div>
    </AdminPermissionGate>
  );
}
