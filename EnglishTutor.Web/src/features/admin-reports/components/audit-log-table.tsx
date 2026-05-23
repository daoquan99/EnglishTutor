"use client";

import { Skeleton } from "@/shared/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/ui/table";
import type { AuditLog } from "../types/admin-reports";

function formatDate(utc: string) {
  return new Date(utc).toLocaleString(undefined, {
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function AuditLogTable({ logs }: { logs?: AuditLog[] }) {
  if (!logs) return <Skeleton className="h-60 w-full" />;

  if (!logs.length) {
    return (
      <p className="py-4 text-center text-sm text-muted-foreground">
        No audit logs found.
      </p>
    );
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Action</TableHead>
          <TableHead>Target</TableHead>
          <TableHead>Entity ID</TableHead>
          <TableHead>Date</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {logs.map((log) => (
          <TableRow key={log.id}>
            <TableCell className="font-medium">{log.action}</TableCell>
            <TableCell>{log.targetEntity}</TableCell>
            <TableCell className="text-xs font-mono">
              {log.targetEntityId.slice(0, 8)}...
            </TableCell>
            <TableCell className="text-xs">
              {formatDate(log.createdAtUtc)}
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
