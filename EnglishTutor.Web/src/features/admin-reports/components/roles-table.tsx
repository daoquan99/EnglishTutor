"use client";

import { Badge } from "@/shared/components/ui/badge";
import { Skeleton } from "@/shared/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/ui/table";
import type { AuthRole } from "../types/admin-reports";

export function RolesTable({ roles }: { roles?: AuthRole[] }) {
  if (!roles) return <Skeleton className="h-60 w-full" />;

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Name</TableHead>
          <TableHead>Description</TableHead>
          <TableHead>Permissions</TableHead>
          <TableHead>Status</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {roles.map((r) => (
          <TableRow key={r.id}>
            <TableCell className="font-medium">{r.name}</TableCell>
            <TableCell className="text-sm">{r.description}</TableCell>
            <TableCell>
              <span className="text-xs text-muted-foreground">
                {r.permissions.length} permissions
              </span>
            </TableCell>
            <TableCell>
              <div className="flex gap-1">
                <Badge variant={r.isEnabled ? "default" : "secondary"}>
                  {r.isEnabled ? "Enabled" : "Disabled"}
                </Badge>
                {r.isSystem && <Badge variant="outline">System</Badge>}
              </div>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
