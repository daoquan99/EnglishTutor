"use client";

import { Check, Copy } from "lucide-react";
import { useMemo, useState } from "react";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from "@/shared/components/ui/sheet";
import { Skeleton } from "@/shared/components/ui/skeleton";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/shared/components/ui/tooltip";
import { useAdminUserDetail } from "../../hooks/use-admin-users";
import { useRoles } from "../../hooks/use-admin-roles";

const dateTimeFormatter = new Intl.DateTimeFormat(undefined, {
  dateStyle: "medium",
  timeStyle: "short",
});

interface UserDetailSheetProps {
  userId: string | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function UserDetailSheet({ userId, open, onOpenChange }: UserDetailSheetProps) {
  const { data: user, isLoading } = useAdminUserDetail(open ? userId : null);
  const { data: allRoles } = useRoles();

  const roleDescriptions = useMemo(() => {
    const map = new Map<string, string>();
    for (const role of allRoles ?? []) {
      if (role.description) map.set(role.id, role.description);
    }
    return map;
  }, [allRoles]);

  const groupedPermissions = useMemo(() => {
    if (!user) return [];
    const groups = new Map<string, string[]>();
    for (const code of user.effectivePermissionCodes) {
      const [prefix] = code.split(".");
      const key = prefix || "other";
      const list = groups.get(key) ?? [];
      list.push(code);
      groups.set(key, list);
    }
    return Array.from(groups.entries())
      .map(([prefix, codes]) => ({
        prefix,
        codes: codes.sort((a, b) => a.localeCompare(b)),
      }))
      .sort((a, b) => a.prefix.localeCompare(b.prefix));
  }, [user]);

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent className="w-full overflow-y-auto sm:max-w-lg">
        <SheetHeader>
          <SheetTitle>User details</SheetTitle>
          <SheetDescription>
            Full account information, assigned roles, and effective permissions.
          </SheetDescription>
        </SheetHeader>

        <div className="mt-6 space-y-6 px-6 pb-6">
          {isLoading || !user ? (
            <Skeleton className="h-60 w-full" />
          ) : (
            <>
              <section className="space-y-2">
                <h3 className="text-sm font-medium text-muted-foreground">
                  Identity
                </h3>
                <div className="space-y-1 rounded-lg border bg-muted/30 p-4 text-sm">
                  <IdRow id={user.id} />
                  <Row label="Display name" value={user.displayName} />
                  <Row label="Email" value={user.email} />
                  <Row
                    label="Status"
                    value={
                      <Badge variant={user.isActive ? "default" : "destructive"}>
                        {user.isActive ? "Active" : "Suspended"}
                      </Badge>
                    }
                  />
                  <Row
                    label="Created"
                    value={dateTimeFormatter.format(new Date(user.createdAtUtc))}
                  />
                  <Row
                    label="Updated"
                    value={dateTimeFormatter.format(new Date(user.updatedAtUtc))}
                  />
                </div>
              </section>

              <section className="space-y-2">
                <h3 className="text-sm font-medium text-muted-foreground">
                  Assigned roles ({user.roles.length})
                </h3>
                {user.roles.length === 0 ? (
                  <p className="text-sm text-muted-foreground">No roles assigned.</p>
                ) : (
                  <div className="space-y-2">
                    {user.roles.map((role) => {
                      const description = roleDescriptions.get(role.roleId);
                      return (
                        <div
                          key={role.roleId}
                          className="rounded-md border bg-card p-3"
                        >
                          <p className="text-sm font-medium">{role.name}</p>
                          {description && (
                            <p className="mt-0.5 text-xs text-muted-foreground">
                              {description}
                            </p>
                          )}
                        </div>
                      );
                    })}
                  </div>
                )}
              </section>

              <section className="space-y-3">
                <h3 className="text-sm font-medium text-muted-foreground">
                  Effective permissions ({user.effectivePermissionCodes.length})
                </h3>
                {groupedPermissions.length === 0 ? (
                  <p className="text-sm text-muted-foreground">
                    No permissions inherited from assigned roles.
                  </p>
                ) : (
                  <div className="space-y-3">
                    {groupedPermissions.map((group) => (
                      <div key={group.prefix} className="space-y-1.5">
                        <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                          {group.prefix} ({group.codes.length})
                        </p>
                        <div className="flex flex-wrap gap-1.5">
                          {group.codes.map((code) => (
                            <Badge
                              key={code}
                              variant="outline"
                              className="font-mono text-xs"
                            >
                              {code}
                            </Badge>
                          ))}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </section>
            </>
          )}
        </div>
      </SheetContent>
    </Sheet>
  );
}

function Row({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex justify-between gap-4 py-1">
      <span className="text-muted-foreground">{label}</span>
      <span className="text-right">{value}</span>
    </div>
  );
}

function IdRow({ id }: { id: string }) {
  const [copied, setCopied] = useState(false);

  const copy = async () => {
    try {
      await navigator.clipboard.writeText(id);
      setCopied(true);
      setTimeout(() => setCopied(false), 1500);
    } catch {
      // Clipboard unavailable; ignore.
    }
  };

  return (
    <div className="flex items-center justify-between gap-4 py-1">
      <span className="text-muted-foreground">ID</span>
      <div className="flex items-center gap-1.5">
        <code className="rounded bg-muted px-1.5 py-0.5 font-mono text-xs">
          {id}
        </code>
        <Tooltip>
          <TooltipTrigger
            render={
              <Button
                type="button"
                variant="ghost"
                size="icon-xs"
                onClick={copy}
                aria-label="Copy user ID"
              />
            }
          >
            {copied ? (
              <Check className="size-3.5 text-success" />
            ) : (
              <Copy className="size-3.5" />
            )}
          </TooltipTrigger>
          <TooltipContent>{copied ? "Copied" : "Copy ID"}</TooltipContent>
        </Tooltip>
      </div>
    </div>
  );
}
