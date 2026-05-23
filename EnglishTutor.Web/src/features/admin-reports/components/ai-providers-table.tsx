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
import type { AiProvider } from "../types/admin-reports";

export function AiProvidersTable({
  providers,
}: {
  providers?: AiProvider[];
}) {
  if (!providers) return <Skeleton className="h-60 w-full" />;

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Provider</TableHead>
          <TableHead>Type</TableHead>
          <TableHead>Models</TableHead>
          <TableHead>Status</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {providers.map((p) => (
          <TableRow key={p.id}>
            <TableCell>
              <div>
                <p className="font-medium">{p.displayName}</p>
                <p className="text-xs text-muted-foreground">
                  {p.providerName}
                </p>
              </div>
            </TableCell>
            <TableCell>{p.providerType}</TableCell>
            <TableCell>
              <span className="text-xs text-muted-foreground">
                {p.models.length} models
              </span>
            </TableCell>
            <TableCell>
              <Badge variant={p.isEnabled ? "default" : "secondary"}>
                {p.isEnabled ? "Active" : "Disabled"}
              </Badge>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
