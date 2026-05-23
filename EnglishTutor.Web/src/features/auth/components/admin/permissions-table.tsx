"use client";

import { MoreHorizontal, Power, PowerOff } from "lucide-react";
import { useMemo } from "react";
import { Badge } from "@/shared/components/ui/badge";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/shared/components/ui/dropdown-menu";
import { AdminDataTable, type AdminDataTableColumn } from "@/shared/admin";
import type { AuthPermission } from "../../types/admin-auth";

type PermissionAction = "toggle";

interface PermissionsTableProps {
  permissions: AuthPermission[] | undefined;
  onAction?: (action: PermissionAction, permission: AuthPermission) => void;
}

export function PermissionsTable({
  permissions,
  onAction,
}: PermissionsTableProps) {
  const columns: AdminDataTableColumn<AuthPermission>[] = useMemo(
    () => [
      {
        id: "code",
        header: "Code",
        cell: (p) => <span className="font-mono text-xs">{p.code}</span>,
      },
      {
        id: "description",
        header: "Description",
        cell: (p) => p.description,
      },
      {
        id: "status",
        header: "Status",
        cell: (p) => (
          <Badge variant={p.isEnabled ? "default" : "secondary"}>
            {p.isEnabled ? "Enabled" : "Disabled"}
          </Badge>
        ),
      },
    ],
    [],
  );

  return (
    <AdminDataTable<AuthPermission>
      data={permissions}
      isLoading={!permissions}
      getRowId={(p) => p.id}
      emptyMessage="Chưa có permission nào."
      columns={columns}
      rowActions={
        onAction
          ? (permission) => (
              <DropdownMenu>
                <DropdownMenuTrigger
                  aria-label={`Actions for ${permission.code}`}
                  className="inline-flex size-8 cursor-pointer items-center justify-center rounded-md outline-none transition-colors hover:bg-muted focus-visible:ring-2 focus-visible:ring-ring"
                >
                  <MoreHorizontal className="size-4" />
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                  <DropdownMenuItem
                    onClick={() => onAction("toggle", permission)}
                    className={permission.isEnabled ? "text-destructive" : ""}
                  >
                    {permission.isEnabled ? (
                      <>
                        <PowerOff className="mr-2 size-3.5" />
                        Tắt permission
                      </>
                    ) : (
                      <>
                        <Power className="mr-2 size-3.5" />
                        Bật permission
                      </>
                    )}
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            )
          : undefined
      }
    />
  );
}
