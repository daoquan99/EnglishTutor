"use client";

import { AdminDeleteConfirmDialog, extractApiErrorMessage } from "@/shared/admin";
import { useDeleteRole } from "../../hooks/use-admin-roles";
import type { AuthRole } from "../../types/admin-auth";

type RoleDeleteDialogProps = {
  role: AuthRole | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export function RoleDeleteDialog({
  role,
  open,
  onOpenChange,
}: RoleDeleteDialogProps) {
  const deleteRole = useDeleteRole();
  const mutationError = deleteRole.error
    ? extractApiErrorMessage(deleteRole.error)
    : null;

  const handleConfirm = async () => {
    if (!role) return;
    try {
      await deleteRole.mutateAsync(role.id);
      onOpenChange(false);
    } catch {
      // toast handled in hook
    }
  };

  if (!role) return null;

  if (role.isSystem) {
    return (
      <AdminDeleteConfirmDialog
        open={open}
        onOpenChange={onOpenChange}
        entityName="System role"
        entityLabel={role.name}
        description="System role không thể xoá. Hành động bị từ chối."
        onConfirm={() => onOpenChange(false)}
        error={{
          title: "Không thể xoá",
          message: "Role này là system role và được bảo vệ.",
        }}
        confirmLabel="Đã hiểu"
      />
    );
  }

  return (
    <AdminDeleteConfirmDialog
      open={open}
      onOpenChange={onOpenChange}
      entityName="role"
      entityLabel={role.name}
      description={`Bạn sắp xoá role ${role.name}. Mọi user đang được gán role này sẽ mất quyền liên quan. Hành động không thể hoàn tác.`}
      onConfirm={handleConfirm}
      isPending={deleteRole.isPending}
      requireTypedConfirm
      error={mutationError}
    />
  );
}
