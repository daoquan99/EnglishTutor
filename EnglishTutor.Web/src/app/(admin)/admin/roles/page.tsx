"use client";

import { Plus, Shield } from "lucide-react";
import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { PageHeader } from "@/shared/components/page-header";
import { AdminAccessDenied, AdminPermissionGate } from "@/shared/admin";
import { PermissionCodes } from "@/features/auth/lib/permission-codes";
import { useRoles } from "@/features/auth/hooks/use-admin-roles";
import { RolesTable } from "@/features/auth/components/admin/roles-table";
import { RoleFormDialog } from "@/features/auth/components/admin/role-form-dialog";
import { RoleDeleteDialog } from "@/features/auth/components/admin/role-delete-dialog";
import { useAdminPermission } from "@/shared/admin";
import type { AuthRole } from "@/features/auth/types/admin-auth";

type ActiveAction = "create" | "edit" | "delete" | null;

export default function RolesPage() {
  const { data } = useRoles();
  const { hasAny } = useAdminPermission();
  const canManage = hasAny([
    PermissionCodes.AuthRolesManage,
    PermissionCodes.FullAccess,
  ]);

  const [selected, setSelected] = useState<AuthRole | null>(null);
  const [active, setActive] = useState<ActiveAction>(null);

  const handleAction = (action: "edit" | "delete", role: AuthRole) => {
    setSelected(role);
    setActive(action);
  };

  const closeAction = () => setActive(null);

  return (
    <AdminPermissionGate
      requireAny={[
        PermissionCodes.AuthRolesRead,
        PermissionCodes.AuthRolesManage,
      ]}
      fallback={<AdminAccessDenied />}
    >
      <div className="page-container page-section">
        <PageHeader
          icon={Shield}
          iconColor="bg-study/10 text-study"
          title="Roles"
          description="Quản lý role và permission gán cho từng role."
          action={
            canManage ? (
              <Button
                onClick={() => {
                  setSelected(null);
                  setActive("create");
                }}
              >
                <Plus className="mr-2 size-4" />
                Tạo role
              </Button>
            ) : undefined
          }
        />
        <div className="mt-6">
          <RolesTable
            roles={data}
            onAction={canManage ? handleAction : undefined}
          />
        </div>

        {canManage ? (
          <>
            <RoleFormDialog
              open={active === "create" || active === "edit"}
              onOpenChange={(open) => !open && closeAction()}
              role={active === "edit" ? selected : null}
            />
            <RoleDeleteDialog
              role={active === "delete" ? selected : null}
              open={active === "delete"}
              onOpenChange={(open) => !open && closeAction()}
            />
          </>
        ) : null}
      </div>
    </AdminPermissionGate>
  );
}
