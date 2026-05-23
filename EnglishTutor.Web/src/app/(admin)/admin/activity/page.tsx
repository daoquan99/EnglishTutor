"use client";

import { Activity } from "lucide-react";
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
import { useLearningActivityReport } from "@/features/admin-reports/hooks/use-admin-reports";

export default function LearningActivityPage() {
  const { data } = useLearningActivityReport();

  return (
    <div className="page-container page-section">
      <PageHeader
        icon={Activity}
        iconColor="bg-study/10 text-study"
        title="Learning Activity"
        description="Daily learning activity reports across all users."
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
                  <TableHead className="text-right">Active Users</TableHead>
                  <TableHead className="text-right">Speaking</TableHead>
                  <TableHead className="text-right">Exercises</TableHead>
                  <TableHead className="text-right">Vocabulary</TableHead>
                  <TableHead className="text-right">Study (min)</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.map((r, i) => (
                  <TableRow key={i}>
                    <TableCell>{r.reportDate}</TableCell>
                    <TableCell className="text-right tabular-nums">{r.totalActiveUsers}</TableCell>
                    <TableCell className="text-right tabular-nums">{r.totalSpeakingSessions}</TableCell>
                    <TableCell className="text-right tabular-nums">{r.totalExercisesCompleted}</TableCell>
                    <TableCell className="text-right tabular-nums">{r.totalVocabularyReviews}</TableCell>
                    <TableCell className="text-right tabular-nums">{r.totalStudyMinutes}</TableCell>
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
