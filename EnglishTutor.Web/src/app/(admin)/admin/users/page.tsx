"use client";

import { Users } from "lucide-react";
import { PageHeader } from "@/shared/components/page-header";
import { useUserOverview } from "@/features/admin-reports/hooks/use-admin-reports";
import { UserOverviewTable } from "@/features/admin-reports/components/user-overview-table";

export default function AdminUsersPage() {
  const { data } = useUserOverview();

  return (
    <div className="page-container page-section">
      <PageHeader
        icon={Users}
        iconColor="bg-info/10 text-info"
        title="User Overview"
        description="Manage users and view activity."
      />
      <div className="mt-6">
        <UserOverviewTable users={data} />
      </div>
    </div>
  );
}
