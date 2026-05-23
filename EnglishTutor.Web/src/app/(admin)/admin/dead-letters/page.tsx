"use client";

import { Inbox } from "lucide-react";
import { PageHeader } from "@/shared/components/page-header";
import { AdminAccessDenied, AdminPermissionGate } from "@/shared/admin";
import { PermissionCodes } from "@/features/auth/lib/permission-codes";
import { useDeadLetters } from "@/features/admin-reports/hooks/use-admin-reports";
import { DeadLetterTable } from "@/features/admin-reports/components/dead-letter-table";

export default function DeadLettersPage() {
  const { data } = useDeadLetters();

  return (
    <AdminPermissionGate
      requireAny={[PermissionCodes.FullAccess]}
      fallback={<AdminAccessDenied />}
    >
      <div className="page-container page-section">
        <PageHeader
          icon={Inbox}
          iconColor="bg-destructive/10 text-destructive"
          title="Dead Letters"
          description="Failed integration events that exceeded retry limits."
        />
        <div className="mt-6">
          <DeadLetterTable messages={data} />
        </div>
      </div>
    </AdminPermissionGate>
  );
}
