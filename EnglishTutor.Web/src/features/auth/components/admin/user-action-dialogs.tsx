"use client";

import { useEffect, useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { Button } from "@/shared/components/ui/button";
import { Checkbox } from "@/shared/components/ui/checkbox";
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
import { Textarea } from "@/shared/components/ui/textarea";
import {
  useRestoreUser,
  useSetUserRoles,
  useSuspendUser,
  useUpdateUser,
} from "../../hooks/use-admin-users";
import type { AuthRole, AuthUserListItem } from "../../types/admin-auth";

// Edit display name dialog

const editSchema = z.object({
  displayName: z
    .string()
    .trim()
    .min(2, "At least 2 characters")
    .max(100, "At most 100 characters"),
});

type EditFormValues = z.infer<typeof editSchema>;

interface EditUserDialogProps {
  user: AuthUserListItem | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function EditUserDialog({ user, open, onOpenChange }: EditUserDialogProps) {
  const updateUser = useUpdateUser(user?.id ?? "");
  const form = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    defaultValues: { displayName: user?.displayName ?? "" },
  });

  useEffect(() => {
    if (open && user) {
      form.reset({ displayName: user.displayName });
    }
  }, [open, user, form]);

  const onSubmit = async (values: EditFormValues) => {
    if (!user) return;
    await updateUser.mutateAsync(values);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Edit display name</DialogTitle>
          <DialogDescription>
            Update how this user is shown across the platform.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-1.5">
            <Label htmlFor="displayName">Display name</Label>
            <Input
              id="displayName"
              autoFocus
              {...form.register("displayName")}
              aria-invalid={!!form.formState.errors.displayName}
            />
            {form.formState.errors.displayName && (
              <p className="text-xs text-destructive">
                {form.formState.errors.displayName.message}
              </p>
            )}
          </div>
          <DialogFooter>
            <Button
              type="button"
              variant="ghost"
              onClick={() => onOpenChange(false)}
              disabled={updateUser.isPending}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={updateUser.isPending}>
              {updateUser.isPending ? "Saving…" : "Save"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

// Suspend user dialog

interface SuspendUserDialogProps {
  user: AuthUserListItem | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function SuspendUserDialog({ user, open, onOpenChange }: SuspendUserDialogProps) {
  const suspendUser = useSuspendUser(user?.id ?? "");
  const [reason, setReason] = useState("");

  useEffect(() => {
    if (open) {
      setReason("");
    }
  }, [open]);

  const onConfirm = async () => {
    if (!user) return;
    await suspendUser.mutateAsync({ reason: reason.trim() || null });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Suspend user</DialogTitle>
          <DialogDescription>
            {user
              ? `${user.displayName} (${user.email}) will be unable to log in until restored.`
              : ""}
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-1.5">
          <Label htmlFor="reason">Reason (optional)</Label>
          <Textarea
            id="reason"
            placeholder="Recorded in the security audit log."
            value={reason}
            onChange={(event) => setReason(event.target.value)}
            rows={3}
            maxLength={500}
          />
        </div>
        <DialogFooter>
          <Button
            type="button"
            variant="ghost"
            onClick={() => onOpenChange(false)}
            disabled={suspendUser.isPending}
          >
            Cancel
          </Button>
          <Button
            type="button"
            variant="destructive"
            onClick={onConfirm}
            disabled={suspendUser.isPending}
          >
            {suspendUser.isPending ? "Suspending…" : "Suspend user"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// Restore user dialog

interface RestoreUserDialogProps {
  user: AuthUserListItem | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function RestoreUserDialog({
  user,
  open,
  onOpenChange,
}: RestoreUserDialogProps) {
  const restoreUser = useRestoreUser();

  const onConfirm = async () => {
    if (!user) return;
    await restoreUser.mutateAsync(user.id);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Restore user</DialogTitle>
          <DialogDescription>
            {user
              ? `${user.displayName} (${user.email}) will regain access to the platform.`
              : ""}
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button
            type="button"
            variant="ghost"
            onClick={() => onOpenChange(false)}
            disabled={restoreUser.isPending}
          >
            Cancel
          </Button>
          <Button
            type="button"
            onClick={onConfirm}
            disabled={restoreUser.isPending}
          >
            {restoreUser.isPending ? "Restoring…" : "Restore user"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// Set roles dialog

interface SetRolesDialogProps {
  user: AuthUserListItem | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  roles: AuthRole[] | undefined;
}

export function SetRolesDialog({
  user,
  open,
  onOpenChange,
  roles,
}: SetRolesDialogProps) {
  const setRoles = useSetUserRoles(user?.id ?? "");
  const [selected, setSelected] = useState<Set<string>>(new Set());

  useEffect(() => {
    if (open && user) {
      setSelected(new Set(user.roleIds));
    }
  }, [open, user]);

  const toggle = (roleId: string) => {
    setSelected((current) => {
      const next = new Set(current);
      if (next.has(roleId)) {
        next.delete(roleId);
      } else {
        next.add(roleId);
      }
      return next;
    });
  };

  const onConfirm = async () => {
    if (!user) return;
    await setRoles.mutateAsync({ roleIds: Array.from(selected) });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Set user roles</DialogTitle>
          <DialogDescription>
            {user
              ? `Roles assigned to ${user.displayName}. Updates replace the full list.`
              : ""}
          </DialogDescription>
        </DialogHeader>
        <div className="max-h-72 space-y-2 overflow-y-auto pr-2">
          {!roles ? (
            <p className="text-sm text-muted-foreground">Loading roles…</p>
          ) : roles.length === 0 ? (
            <p className="text-sm text-muted-foreground">No roles defined.</p>
          ) : (
            roles
              .filter((role) => role.isEnabled)
              .map((role) => (
                <label
                  key={role.id}
                  className="flex cursor-pointer items-start gap-3 rounded-md border p-3 transition-colors hover:bg-muted/50"
                >
                  <Checkbox
                    checked={selected.has(role.id)}
                    onCheckedChange={() => toggle(role.id)}
                    className="mt-0.5"
                  />
                  <div className="min-w-0 flex-1">
                    <p className="text-sm font-medium">{role.name}</p>
                    {role.description && (
                      <p className="text-xs text-muted-foreground">{role.description}</p>
                    )}
                  </div>
                </label>
              ))
          )}
        </div>
        <DialogFooter>
          <Button
            type="button"
            variant="ghost"
            onClick={() => onOpenChange(false)}
            disabled={setRoles.isPending}
          >
            Cancel
          </Button>
          <Button
            type="button"
            onClick={onConfirm}
            disabled={setRoles.isPending}
          >
            {setRoles.isPending ? "Saving…" : "Save roles"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
