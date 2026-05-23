"use client";

import { Search } from "lucide-react";
import { useMemo, useState } from "react";
import { Checkbox } from "@/shared/components/ui/checkbox";
import { Input } from "@/shared/components/ui/input";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { cn } from "@/shared/lib/utils";
import { usePermissions } from "../../hooks/use-admin-permissions";
import type { AuthPermission } from "../../types/admin-auth";

type PermissionPickerProps = {
  value: string[];
  onChange: (next: string[]) => void;
  disabled?: boolean;
};

type PermissionGroup = {
  prefix: string;
  permissions: AuthPermission[];
};

export function PermissionPicker({
  value,
  onChange,
  disabled,
}: PermissionPickerProps) {
  const { data: permissions, isLoading } = usePermissions();
  const [search, setSearch] = useState("");

  const selectedSet = useMemo(() => new Set(value), [value]);

  const groups = useMemo<PermissionGroup[]>(() => {
    const enabled = (permissions ?? []).filter((p) => p.isEnabled);
    const term = search.trim().toLowerCase();
    const filtered = term
      ? enabled.filter(
          (p) =>
            p.code.toLowerCase().includes(term) ||
            p.description.toLowerCase().includes(term),
        )
      : enabled;

    const map = new Map<string, AuthPermission[]>();
    for (const permission of filtered) {
      const [prefix] = permission.code.split(".");
      const key = prefix || "other";
      const list = map.get(key) ?? [];
      list.push(permission);
      map.set(key, list);
    }
    return Array.from(map.entries())
      .map(([prefix, perms]) => ({
        prefix,
        permissions: perms.sort((a, b) => a.code.localeCompare(b.code)),
      }))
      .sort((a, b) => a.prefix.localeCompare(b.prefix));
  }, [permissions, search]);

  const togglePermission = (id: string) => {
    const next = new Set(selectedSet);
    if (next.has(id)) {
      next.delete(id);
    } else {
      next.add(id);
    }
    onChange(Array.from(next));
  };

  const toggleGroup = (group: PermissionGroup, allSelected: boolean) => {
    const groupIds = group.permissions.map((p) => p.id);
    if (allSelected) {
      onChange(value.filter((id) => !groupIds.includes(id)));
    } else {
      const next = new Set(selectedSet);
      groupIds.forEach((id) => next.add(id));
      onChange(Array.from(next));
    }
  };

  if (isLoading) {
    return <Skeleton className="h-64 w-full" />;
  }

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between gap-3">
        <div className="relative w-full max-w-xs">
          <Search className="absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Tìm permission…"
            className="pl-8"
            disabled={disabled}
          />
        </div>
        <span className="text-xs text-muted-foreground">
          Đã chọn {value.length}
        </span>
      </div>

      <div className="max-h-80 space-y-3 overflow-y-auto rounded-md border bg-muted/20 p-3">
        {groups.length === 0 ? (
          <p className="py-6 text-center text-sm text-muted-foreground">
            Không có permission phù hợp.
          </p>
        ) : (
          groups.map((group) => {
            const allSelected = group.permissions.every((p) =>
              selectedSet.has(p.id),
            );
            const someSelected = group.permissions.some((p) =>
              selectedSet.has(p.id),
            );
            return (
              <div key={group.prefix} className="space-y-1.5">
                <button
                  type="button"
                  onClick={() => toggleGroup(group, allSelected)}
                  disabled={disabled}
                  className="flex w-full items-center justify-between rounded px-1 py-1 text-left text-xs font-semibold uppercase tracking-wide text-muted-foreground hover:bg-muted/40"
                >
                  <span className="flex items-center gap-2">
                    <Checkbox
                      checked={allSelected}
                      onCheckedChange={() => toggleGroup(group, allSelected)}
                      disabled={disabled}
                      aria-label={`Toggle ${group.prefix} group`}
                      data-state={
                        allSelected
                          ? "checked"
                          : someSelected
                            ? "indeterminate"
                            : "unchecked"
                      }
                    />
                    {group.prefix}
                  </span>
                  <span className="text-[10px] normal-case text-muted-foreground/70">
                    {group.permissions.filter((p) => selectedSet.has(p.id)).length}/
                    {group.permissions.length}
                  </span>
                </button>
                <ul className="space-y-1 pl-4">
                  {group.permissions.map((permission) => {
                    const checked = selectedSet.has(permission.id);
                    return (
                      <li key={permission.id}>
                        <label
                          className={cn(
                            "flex cursor-pointer items-start gap-2 rounded px-2 py-1 transition-colors hover:bg-muted/40",
                            disabled && "cursor-not-allowed opacity-60",
                          )}
                        >
                          <Checkbox
                            checked={checked}
                            onCheckedChange={() =>
                              togglePermission(permission.id)
                            }
                            disabled={disabled}
                            className="mt-0.5"
                          />
                          <div className="min-w-0 flex-1">
                            <p className="font-mono text-xs">{permission.code}</p>
                            <p className="text-xs text-muted-foreground">
                              {permission.description}
                            </p>
                          </div>
                        </label>
                      </li>
                    );
                  })}
                </ul>
              </div>
            );
          })
        )}
      </div>
    </div>
  );
}
