"use client";

import { useMemo } from "react";
import { Loader2, RotateCcw } from "lucide-react";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import { AdminDataTable, type AdminDataTableColumn } from "@/shared/admin";
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

  const columns: AdminDataTableColumn<DeadLetterMessage>[] = useMemo(
    () => [
      {
        id: "eventType",
        header: "Event Type",
        cell: (m) => <span className="font-mono text-xs">{m.eventType}</span>,
      },
      { id: "source", header: "Source", cell: (m) => m.sourceModule },
      {
        id: "retries",
        header: "Retries",
        align: "right",
        cell: (m) => <span className="tabular-nums">{m.retryCount}</span>,
      },
      {
        id: "status",
        header: "Status",
        cell: (m) => <Badge variant="secondary">{m.status}</Badge>,
      },
      {
        id: "failedAt",
        header: "Failed At",
        cell: (m) => <span className="text-xs">{formatDate(m.failedAtUtc)}</span>,
      },
    ],
    [],
  );

  return (
    <AdminDataTable<DeadLetterMessage>
      data={messages}
      isLoading={!messages}
      getRowId={(m) => m.id}
      emptyMessage="Không có dead letter nào."
      columns={columns}
      rowActions={(m) => (
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
      )}
    />
  );
}
