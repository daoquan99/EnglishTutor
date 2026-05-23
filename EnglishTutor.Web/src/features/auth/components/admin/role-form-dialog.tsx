"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import { useForm, Controller } from "react-hook-form";
import { AdminCrudDialog, extractApiErrorMessage } from "@/shared/admin";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { Switch } from "@/shared/components/ui/switch";
import { Textarea } from "@/shared/components/ui/textarea";
import { useCreateRole, useUpdateRole } from "../../hooks/use-admin-roles";
import {
  roleFormSchema,
  type RoleFormValues,
} from "../../schemas/role-schema";
import type { AuthRole } from "../../types/admin-auth";
import { PermissionPicker } from "./permission-picker";

type RoleFormDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  /** When provided, dialog runs in `edit` mode and prefills from this role. */
  role?: AuthRole | null;
};

const DEFAULT_VALUES: RoleFormValues = {
  name: "",
  description: "",
  isEnabled: true,
  permissionIds: [],
};

export function RoleFormDialog({ open, onOpenChange, role }: RoleFormDialogProps) {
  const mode: "create" | "edit" = role ? "edit" : "create";
  const create = useCreateRole();
  const update = useUpdateRole();
  const mutation = mode === "create" ? create : update;

  const form = useForm<RoleFormValues>({
    resolver: zodResolver(roleFormSchema),
    defaultValues: DEFAULT_VALUES,
  });

  useEffect(() => {
    if (!open) return;
    if (role) {
      form.reset({
        name: role.name,
        description: role.description,
        isEnabled: role.isEnabled,
        permissionIds: role.permissions.map((p) => p.id),
      });
    } else {
      form.reset(DEFAULT_VALUES);
    }
  }, [open, role, form]);

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      if (role) {
        await update.mutateAsync({ id: role.id, data: values });
      } else {
        await create.mutateAsync(values);
      }
      onOpenChange(false);
    } catch {
      // useAdminToast already surfaced the error
    }
  });

  const errors = form.formState.errors;
  const mutationError = mutation.error
    ? extractApiErrorMessage(mutation.error)
    : null;

  return (
    <AdminCrudDialog
      mode={mode}
      open={open}
      onOpenChange={onOpenChange}
      title={role ? `Sửa role: ${role.name}` : "Tạo role mới"}
      description={
        role
          ? "Cập nhật tên, mô tả, trạng thái và permission của role."
          : "Tạo một role mới và gán permission ban đầu."
      }
      onSubmit={onSubmit}
      isSubmitting={mutation.isPending}
      error={mutationError}
    >
      <div className="grid gap-3 sm:grid-cols-2">
        <div className="space-y-1.5">
          <Label htmlFor="role-name">Tên</Label>
          <Input
            id="role-name"
            placeholder="vd: content_editor"
            disabled={role?.isSystem || mutation.isPending}
            {...form.register("name")}
          />
          {errors.name ? (
            <p className="text-xs text-destructive">{errors.name.message}</p>
          ) : (
            <p className="text-[11px] text-muted-foreground">
              Chữ thường, số, dấu - và _.
            </p>
          )}
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="role-enabled">Trạng thái</Label>
          <div className="flex items-center gap-2 rounded-md border px-3 py-2">
            <Controller
              control={form.control}
              name="isEnabled"
              render={({ field }) => (
                <Switch
                  id="role-enabled"
                  checked={field.value}
                  onCheckedChange={field.onChange}
                  disabled={role?.isSystem || mutation.isPending}
                />
              )}
            />
            <span className="text-sm text-muted-foreground">
              {form.watch("isEnabled") ? "Đang bật" : "Đang tắt"}
            </span>
          </div>
        </div>
      </div>

      <div className="space-y-1.5">
        <Label htmlFor="role-description">Mô tả</Label>
        <Textarea
          id="role-description"
          rows={2}
          maxLength={300}
          disabled={mutation.isPending}
          {...form.register("description")}
        />
        {errors.description ? (
          <p className="text-xs text-destructive">{errors.description.message}</p>
        ) : null}
      </div>

      <div className="space-y-1.5">
        <Label>Permissions</Label>
        <Controller
          control={form.control}
          name="permissionIds"
          render={({ field }) => (
            <PermissionPicker
              value={field.value}
              onChange={field.onChange}
              disabled={mutation.isPending}
            />
          )}
        />
        {errors.permissionIds ? (
          <p className="text-xs text-destructive">
            {errors.permissionIds.message}
          </p>
        ) : null}
      </div>

      {role?.isSystem ? (
        <p className="rounded-md border border-amber-500/30 bg-amber-50 px-3 py-2 text-xs text-amber-900 dark:bg-amber-950/30 dark:text-amber-200">
          Đây là system role. Tên và trạng thái không thể chỉnh sửa.
        </p>
      ) : null}
    </AdminCrudDialog>
  );
}
