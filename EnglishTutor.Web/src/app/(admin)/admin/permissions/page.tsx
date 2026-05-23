"use client";

import { Key } from "lucide-react";
import { useState } from "react";
import { PageHeader } from "@/shared/components/page-header";
import { AdminAccessDenied, AdminPermissionGate, useAdminPermission } from "@/shared/admin";
import { PermissionCodes } from "@/features/auth/lib/permission-codes";
import { usePermissions } from "@/features/auth/hooks/use-admin-permissions";
import { PermissionsTable } from "@/features/auth/components/admin/permissions-table";
import { PermissionToggleDialog } from "@/features/auth/components/admin/permission-toggle-dialog";
import type { AuthPermission } from "@/features/auth/types/admin-auth";

export default function PermissionsPage() {
  const { data } = usePermissions();
  const { hasAny } = useAdminPermission();
  const canManage = hasAny([
    PermissionCodes.AuthPermissionsManage,
    PermissionCodes.FullAccess,
  ]);

  const [selected, setSelected] = useState<AuthPermission | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);

  const handleAction = (_action: "toggle", permission: AuthPermission) => {
    setSelected(permission);
    setDialogOpen(true);
  };

  return (
    <AdminPermissionGate
      requireAny={[
        PermissionCodes.AuthPermissionsRead,
        PermissionCodes.AuthPermissionsManage,
      ]}
      fallback={<AdminAccessDenied />}
    >
      <div className="page-container page-section">
        <PageHeader
          icon={Key}
          iconColor="bg-warning/10 text-warning"
          title="Permissions"
          description="Bật / tắt từng permission. Permission codes được khai báo ở backend; UI này chỉ điều khiển trạng thái hiệu lực."
        />
        <div className="mt-6">
          <PermissionsTable
            permissions={data}
            onAction={canManage ? handleAction : undefined}
          />
        </div>

        {canManage ? (
          <PermissionToggleDialog
            permission={dialogOpen ? selected : null}
            open={dialogOpen}
            onOpenChange={setDialogOpen}
          />
        ) : null}
      </div>
    </AdminPermissionGate>
  );
}
