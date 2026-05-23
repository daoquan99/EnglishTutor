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
import type { AiRuntimeRoute } from "../types/admin-reports";

export function AiRoutesTable({ routes }: { routes?: AiRuntimeRoute[] }) {
  if (!routes) return <Skeleton className="h-60 w-full" />;

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Task Type</TableHead>
          <TableHead>Capability</TableHead>
          <TableHead>Preferred</TableHead>
          <TableHead>Fallback</TableHead>
          <TableHead>Status</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {routes.map((r) => (
          <TableRow key={r.id}>
            <TableCell className="font-medium">{r.taskType}</TableCell>
            <TableCell>{r.capability}</TableCell>
            <TableCell className="text-xs">
              {r.preferredProviderName}/{r.preferredModelCode}
            </TableCell>
            <TableCell className="text-xs text-muted-foreground">
              {r.fallbackProviderName
                ? `${r.fallbackProviderName}/${r.fallbackModelCode}`
                : "None"}
            </TableCell>
            <TableCell>
              <Badge variant={r.isActive ? "default" : "secondary"}>
                {r.isActive ? "Active" : "Inactive"}
              </Badge>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
