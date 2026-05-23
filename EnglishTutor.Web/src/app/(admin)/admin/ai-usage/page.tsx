"use client";

import { Brain } from "lucide-react";
import { Skeleton } from "@/shared/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/ui/table";
import { PageHeader } from "@/shared/components/page-header";
import { useAiUsageReport } from "@/features/admin-reports/hooks/use-admin-reports";

export default function AiUsagePage() {
  const { data } = useAiUsageReport();

  return (
    <div className="page-container page-section">
      <PageHeader
        icon={Brain}
        iconColor="bg-speaking/10 text-speaking"
        title="AI Usage"
        description="Token usage, latency, and estimated costs."
      />
      <div className="mt-6">
        {!data ? (
          <Skeleton className="h-60 w-full rounded-xl" />
        ) : (
          <div className="overflow-hidden rounded-xl border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Date</TableHead>
                  <TableHead>Model</TableHead>
                  <TableHead>Task</TableHead>
                  <TableHead className="text-right">Requests</TableHead>
                  <TableHead className="text-right">Tokens</TableHead>
                  <TableHead className="text-right">Cost (USD)</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.map((r, i) => (
                  <TableRow key={i}>
                    <TableCell>{r.reportDate}</TableCell>
                    <TableCell>{r.modelType}</TableCell>
                    <TableCell>{r.taskType}</TableCell>
                    <TableCell className="text-right tabular-nums">{r.totalRequests}</TableCell>
                    <TableCell className="text-right tabular-nums">{r.totalTokens.toLocaleString()}</TableCell>
                    <TableCell className="text-right tabular-nums">${r.estimatedCostUsd.toFixed(2)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        )}
      </div>
    </div>
  );
}
