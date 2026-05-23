"use client";

import { useEffect, useState } from "react";
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
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";

type AdminDeleteConfirmDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  entityName: string;
  entityLabel: string;
  onConfirm: () => void | Promise<void>;
  isPending?: boolean;
  description?: string;
  requireTypedConfirm?: boolean;
  error?: { title?: string; message: string } | null;
  confirmLabel?: string;
};

export function AdminDeleteConfirmDialog({
  open,
  onOpenChange,
  entityName,
  entityLabel,
  onConfirm,
  isPending,
  description,
  requireTypedConfirm = false,
  error,
  confirmLabel = "Xoá",
}: AdminDeleteConfirmDialogProps) {
  const [typed, setTyped] = useState("");

  useEffect(() => {
    if (!open) {
      setTyped("");
    }
  }, [open]);

  const canConfirm = requireTypedConfirm ? typed === entityLabel : true;

  return (
    <Dialog open={open} onOpenChange={isPending ? undefined : onOpenChange}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-destructive">
            <AlertTriangle className="size-5" />
            Xoá {entityName}
          </DialogTitle>
          <DialogDescription>
            {description ??
              `Bạn sắp xoá ${entityName.toLowerCase()} `}
            <span className="font-semibold text-foreground">{entityLabel}</span>
            {!description ? ". Hành động này không thể hoàn tác." : null}
          </DialogDescription>
        </DialogHeader>

        {error ? (
          <Alert variant="destructive">
            <AlertTitle>{error.title ?? "Không thể xoá"}</AlertTitle>
            <AlertDescription>{error.message}</AlertDescription>
          </Alert>
        ) : null}

        {requireTypedConfirm ? (
          <div className="space-y-2">
            <Label htmlFor="confirm-input">
              Nhập <span className="font-mono text-foreground">{entityLabel}</span> để xác nhận
            </Label>
            <Input
              id="confirm-input"
              value={typed}
              onChange={(event) => setTyped(event.target.value)}
              autoComplete="off"
              disabled={isPending}
            />
          </div>
        ) : null}

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={isPending}
          >
            Huỷ
          </Button>
          <Button
            type="button"
            variant="destructive"
            disabled={!canConfirm || isPending}
            onClick={() => {
              void onConfirm();
            }}
          >
            {isPending ? <Loader2 className="mr-2 size-4 animate-spin" /> : null}
            {confirmLabel}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
