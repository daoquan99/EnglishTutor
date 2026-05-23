"use client";

import { Shield } from "lucide-react";
import { PageHeader } from "@/shared/components/page-header";
import { useRoles } from "@/features/admin-reports/hooks/use-admin-auth";
import { RolesTable } from "@/features/admin-reports/components/roles-table";

export default function RolesPage() {
  const { data } = useRoles();

  return (
    <div className="page-container page-section">
      <PageHeader
        icon={Shield}
        iconColor="bg-study/10 text-study"
        title="Roles"
        description="Manage roles and their permissions."
      />
      <div className="mt-6">
        <RolesTable roles={data} />
      </div>
    </div>
  );
}
