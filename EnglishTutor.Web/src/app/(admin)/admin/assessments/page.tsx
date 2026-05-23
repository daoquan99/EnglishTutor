"use client";

import { BookCheck } from "lucide-react";
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
import { useAssessmentPassRates } from "@/features/admin-reports/hooks/use-admin-reports";

export default function AdminAssessmentsPage() {
  const { data } = useAssessmentPassRates();

  return (
    <div className="page-container page-section">
      <PageHeader
        icon={BookCheck}
        iconColor="bg-vocabulary/10 text-vocabulary"
        title="Assessment Pass Rates"
        description="Assessment completion and scoring statistics."
      />
      <div className="mt-6">
        {!data ? (
          <Skeleton className="h-60 w-full rounded-xl" />
        ) : (
          <div className="overflow-hidden rounded-xl border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Type</TableHead>
                  <TableHead>Level</TableHead>
                  <TableHead className="text-right">Attempts</TableHead>
                  <TableHead className="text-right">Pass Rate</TableHead>
                  <TableHead className="text-right">Avg Score</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.map((r, i) => (
                  <TableRow key={i}>
                    <TableCell>{r.assessmentType}</TableCell>
                    <TableCell>{r.forLevel}</TableCell>
                    <TableCell className="text-right tabular-nums">{r.totalAttempts}</TableCell>
                    <TableCell className="text-right tabular-nums">{(r.passRate * 100).toFixed(1)}%</TableCell>
                    <TableCell className="text-right tabular-nums">{r.averageScore.toFixed(1)}</TableCell>
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
