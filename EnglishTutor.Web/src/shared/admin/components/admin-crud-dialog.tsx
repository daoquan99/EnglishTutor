"use client";

import type { FormEvent, ReactNode } from "react";
import { AlertCircle, Loader2 } from "lucide-react";
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

type AdminCrudDialogProps = {
  mode: "create" | "edit";
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description?: string;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  isSubmitting?: boolean;
  error?: { title?: string; message: string } | null;
  submitLabel?: string;
  cancelLabel?: string;
  children: ReactNode;
};

export function AdminCrudDialog({
  mode,
  open,
  onOpenChange,
  title,
  description,
  onSubmit,
  isSubmitting,
  error,
  submitLabel,
  cancelLabel = "Huỷ",
  children,
}: AdminCrudDialogProps) {
  const defaultSubmit = mode === "create" ? "Tạo" : "Lưu thay đổi";
  return (
    <Dialog open={open} onOpenChange={isSubmitting ? undefined : onOpenChange}>
      <DialogContent className="max-w-2xl">
        <form onSubmit={onSubmit} className="space-y-4">
          <DialogHeader>
            <DialogTitle>{title}</DialogTitle>
            {description ? <DialogDescription>{description}</DialogDescription> : null}
          </DialogHeader>

          {error ? (
            <Alert variant="destructive">
              <AlertCircle className="size-4" />
              <AlertTitle>{error.title ?? "Có lỗi xảy ra"}</AlertTitle>
              <AlertDescription>{error.message}</AlertDescription>
            </Alert>
          ) : null}

          <div className="space-y-4">{children}</div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={isSubmitting}
            >
              {cancelLabel}
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? <Loader2 className="mr-2 size-4 animate-spin" /> : null}
              {submitLabel ?? defaultSubmit}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
