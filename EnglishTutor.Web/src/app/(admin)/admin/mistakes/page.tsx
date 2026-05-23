"use client";

import { AlertTriangle } from "lucide-react";
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
import { useCommonMistakesReport } from "@/features/admin-reports/hooks/use-admin-reports";

export default function AdminMistakesPage() {
  const { data } = useCommonMistakesReport();

  return (
    <div className="page-container page-section">
      <PageHeader
        icon={AlertTriangle}
        iconColor="bg-destructive/10 text-destructive"
        title="Common Mistakes"
        description="Most frequent mistake patterns across all users."
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
                  <TableHead>Category</TableHead>
                  <TableHead className="text-right">Occurrences</TableHead>
                  <TableHead className="text-right">Users</TableHead>
                  <TableHead>Example</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.map((m, i) => (
                  <TableRow key={i}>
                    <TableCell>{m.mistakeType}</TableCell>
                    <TableCell>{m.category}</TableCell>
                    <TableCell className="text-right tabular-nums">{m.occurrenceCount}</TableCell>
                    <TableCell className="text-right tabular-nums">{m.affectedUsers}</TableCell>
                    <TableCell className="text-xs">
                      <span className="text-destructive line-through">
                        {m.exampleOriginal}
                      </span>{" "}
                      <span className="text-success">{m.exampleCorrected}</span>
                    </TableCell>
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
