"use client";

import { AlertTriangle, Loader2 } from "lucide-react";
import { Alert, AlertDescription, AlertTitle } from "@/shared/components/ui/alert";
import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { extractApiErrorMessage } from "@/shared/admin";
import { useUpdatePermission } from "../../hooks/use-admin-permissions";
import type { AuthPermission } from "../../types/admin-auth";

type PermissionToggleDialogProps = {
  permission: AuthPermission | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export function PermissionToggleDialog({
  permission,
  open,
  onOpenChange,
}: PermissionToggleDialogProps) {
  const updatePermission = useUpdatePermission();

  if (!permission) return null;

  const nextEnabled = !permission.isEnabled;
  const actionLabel = nextEnabled ? "Bật" : "Tắt";
  const error = updatePermission.error
    ? extractApiErrorMessage(updatePermission.error)
    : null;

  const handleConfirm = async () => {
    try {
      await updatePermission.mutateAsync({
        id: permission.id,
        data: {
          description: permission.description,
          isEnabled: nextEnabled,
        },
      });
      onOpenChange(false);
    } catch {
      // toast handled in hook
    }
  };

  return (
    <Dialog
      open={open}
      onOpenChange={updatePermission.isPending ? undefined : onOpenChange}
    >
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle
            className={nextEnabled ? "" : "flex items-center gap-2 text-destructive"}
          >
            {!nextEnabled ? <AlertTriangle className="size-5" /> : null}
            {actionLabel} permission
          </DialogTitle>
          <DialogDescription>
            Bạn sắp{" "}
            <span className="font-semibold text-foreground">
              {actionLabel.toLowerCase()}
            </span>{" "}
            permission{" "}
            <code className="rounded bg-muted px-1.5 py-0.5 font-mono text-xs">
              {permission.code}
            </code>
            .
            {!nextEnabled
              ? " Permission này sẽ không còn hiệu lực với mọi role đang gán nó."
              : " Permission sẽ có hiệu lực ngay lập tức cho mọi role đang gán."}
          </DialogDescription>
        </DialogHeader>

        {error ? (
          <Alert variant="destructive">
            <AlertTitle>Không thể cập nhật</AlertTitle>
            <AlertDescription>{error.message}</AlertDescription>
          </Alert>
        ) : null}

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={updatePermission.isPending}
          >
            Huỷ
          </Button>
          <Button
            type="button"
            variant={nextEnabled ? "default" : "destructive"}
            onClick={handleConfirm}
            disabled={updatePermission.isPending}
          >
            {updatePermission.isPending ? (
              <Loader2 className="mr-2 size-4 animate-spin" />
            ) : null}
            {actionLabel} permission
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
