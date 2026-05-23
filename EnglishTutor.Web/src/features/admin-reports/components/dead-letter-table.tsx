"use client";

import { Loader2, RotateCcw } from "lucide-react";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import { Skeleton } from "@/shared/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/ui/table";
import type { DeadLetterMessage } from "../types/admin-reports";
import { useReprocessDeadLetter } from "../hooks/use-admin-reports";

function formatDate(utc: string) {
  return new Date(utc).toLocaleString(undefined, {
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function DeadLetterTable({
  messages,
}: {
  messages?: DeadLetterMessage[];
}) {
  const reprocess = useReprocessDeadLetter();

  if (!messages) return <Skeleton className="h-60 w-full" />;

  if (!messages.length) {
    return (
      <p className="py-4 text-center text-sm text-muted-foreground">
        No dead letters.
      </p>
    );
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Event Type</TableHead>
          <TableHead>Source</TableHead>
          <TableHead>Retries</TableHead>
          <TableHead>Status</TableHead>
          <TableHead>Failed At</TableHead>
          <TableHead />
        </TableRow>
      </TableHeader>
      <TableBody>
        {messages.map((m) => (
          <TableRow key={m.id}>
            <TableCell className="text-xs font-mono">{m.eventType}</TableCell>
            <TableCell>{m.sourceModule}</TableCell>
            <TableCell>{m.retryCount}</TableCell>
            <TableCell>
              <Badge variant="secondary">{m.status}</Badge>
            </TableCell>
            <TableCell className="text-xs">
              {formatDate(m.failedAtUtc)}
            </TableCell>
            <TableCell>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => reprocess.mutate(m.id)}
                disabled={reprocess.isPending}
              >
                {reprocess.isPending ? (
                  <Loader2 className="h-3 w-3 animate-spin" />
                ) : (
                  <RotateCcw className="h-3 w-3" />
                )}
                Retry
              </Button>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
