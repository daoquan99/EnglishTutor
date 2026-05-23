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
import type { AuthPermission } from "../types/admin-reports";

export function PermissionsTable({
  permissions,
}: {
  permissions?: AuthPermission[];
}) {
  if (!permissions) return <Skeleton className="h-60 w-full" />;

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Code</TableHead>
          <TableHead>Description</TableHead>
          <TableHead>Status</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {permissions.map((p) => (
          <TableRow key={p.id}>
            <TableCell className="font-mono text-xs">{p.code}</TableCell>
            <TableCell>{p.description}</TableCell>
            <TableCell>
              <Badge variant={p.isEnabled ? "default" : "secondary"}>
                {p.isEnabled ? "Enabled" : "Disabled"}
              </Badge>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
