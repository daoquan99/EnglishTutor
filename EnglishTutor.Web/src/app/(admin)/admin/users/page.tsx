"use client";

import { Users } from "lucide-react";
import { useState } from "react";
import { PageHeader } from "@/shared/components/page-header";
import { AdminAccessDenied, AdminPermissionGate } from "@/shared/admin";
import { PermissionCodes } from "@/features/auth/lib/permission-codes";
import {
  EditUserDialog,
  RestoreUserDialog,
  SetRolesDialog,
  SuspendUserDialog,
} from "@/features/auth/components/admin/user-action-dialogs";
import { UserDetailSheet } from "@/features/auth/components/admin/user-detail-sheet";
import { UsersTable } from "@/features/auth/components/admin/users-table";
import { useRoles } from "@/features/auth/hooks/use-admin-roles";
import type { AuthUserListItem } from "@/features/auth/types/admin-auth";

type ActiveAction = "view" | "edit" | "roles" | "suspend" | "restore" | null;

export default function AdminUsersPage() {
  const { data: roles } = useRoles();
  const [selected, setSelected] = useState<AuthUserListItem | null>(null);
  const [activeAction, setActiveAction] = useState<ActiveAction>(null);

  const handleAction = (
    action: "view" | "edit" | "roles" | "suspend" | "restore",
    user: AuthUserListItem,
  ) => {
    setSelected(user);
    setActiveAction(action);
  };

  const closeAction = () => setActiveAction(null);

  return (
    <AdminPermissionGate
      requireAny={[
        PermissionCodes.AuthUsersRead,
        PermissionCodes.AuthUsersManage,
      ]}
      fallback={<AdminAccessDenied />}
    >
      <div className="page-container page-section">
        <PageHeader
          icon={Users}
          iconColor="bg-info/10 text-info"
          title="User management"
          description="Search, suspend, restore, and assign roles to platform users."
        />
        <div className="mt-6">
          <UsersTable roles={roles} onAction={handleAction} />
        </div>

        <UserDetailSheet
          userId={selected?.id ?? null}
          open={activeAction === "view"}
          onOpenChange={(open) => !open && closeAction()}
        />
        <EditUserDialog
          user={selected}
          open={activeAction === "edit"}
          onOpenChange={(open) => !open && closeAction()}
        />
        <SetRolesDialog
          user={selected}
          roles={roles}
          open={activeAction === "roles"}
          onOpenChange={(open) => !open && closeAction()}
        />
        <SuspendUserDialog
          user={selected}
          open={activeAction === "suspend"}
          onOpenChange={(open) => !open && closeAction()}
        />
        <RestoreUserDialog
          user={selected}
          open={activeAction === "restore"}
          onOpenChange={(open) => !open && closeAction()}
        />
      </div>
    </AdminPermissionGate>
  );
}
