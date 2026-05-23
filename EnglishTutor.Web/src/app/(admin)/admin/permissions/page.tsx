"use client";

import { Key } from "lucide-react";
import { PageHeader } from "@/shared/components/page-header";
import { usePermissions } from "@/features/admin-reports/hooks/use-admin-auth";
import { PermissionsTable } from "@/features/admin-reports/components/permissions-table";

export default function PermissionsPage() {
  const { data } = usePermissions();

  return (
    <div className="page-container page-section">
      <PageHeader
        icon={Key}
        iconColor="bg-warning/10 text-warning"
        title="Permissions"
        description="View and manage system permissions."
      />
      <div className="mt-6">
        <PermissionsTable permissions={data} />
      </div>
    </div>
  );
}
