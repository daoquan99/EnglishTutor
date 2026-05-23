"use client";

import { Edit, MoreHorizontal, Trash2 } from "lucide-react";
import { useMemo } from "react";
import { Badge } from "@/shared/components/ui/badge";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/shared/components/ui/dropdown-menu";
import { AdminDataTable, type AdminDataTableColumn } from "@/shared/admin";
import type { AuthRole } from "../../types/admin-auth";

type RoleAction = "edit" | "delete";

interface RolesTableProps {
  roles: AuthRole[] | undefined;
  onAction?: (action: RoleAction, role: AuthRole) => void;
}

export function RolesTable({ roles, onAction }: RolesTableProps) {
  const columns: AdminDataTableColumn<AuthRole>[] = useMemo(
    () => [
      {
        id: "name",
        header: "Name",
        cell: (r) => <span className="font-medium">{r.name}</span>,
      },
      {
        id: "description",
        header: "Description",
        cell: (r) => <span className="text-sm">{r.description}</span>,
      },
      {
        id: "permissions",
        header: "Permissions",
        align: "right",
        cell: (r) => (
          <span className="text-xs text-muted-foreground">
            {r.permissions.length} permissions
          </span>
        ),
      },
      {
        id: "status",
        header: "Status",
        cell: (r) => (
          <div className="flex gap-1">
            <Badge variant={r.isEnabled ? "default" : "secondary"}>
              {r.isEnabled ? "Enabled" : "Disabled"}
            </Badge>
            {r.isSystem && <Badge variant="outline">System</Badge>}
          </div>
        ),
      },
    ],
    [],
  );

  return (
    <AdminDataTable<AuthRole>
      data={roles}
      isLoading={!roles}
      getRowId={(r) => r.id}
      emptyMessage="Chưa có role nào."
      columns={columns}
      rowActions={
        onAction
          ? (role) => (
              <DropdownMenu>
                <DropdownMenuTrigger
                  aria-label={`Actions for ${role.name}`}
                  className="inline-flex size-8 cursor-pointer items-center justify-center rounded-md outline-none transition-colors hover:bg-muted focus-visible:ring-2 focus-visible:ring-ring"
                >
                  <MoreHorizontal className="size-4" />
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                  <DropdownMenuItem onClick={() => onAction("edit", role)}>
                    <Edit className="mr-2 size-3.5" />
                    Sửa role
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem
                    onClick={() => onAction("delete", role)}
                    className="text-destructive"
                    disabled={role.isSystem}
                  >
                    <Trash2 className="mr-2 size-3.5" />
                    Xoá role
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            )
          : undefined
      }
    />
  );
}
