"use client";

import { use } from "react";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { useAssessmentAttempt } from "@/features/assessments/hooks/use-assessment-attempt";
import { useAssessmentResult } from "@/features/assessments/hooks/use-assessment-result";
import { AssessmentPlayer } from "@/features/assessments/components/assessment-player";
import { AssessmentResultView } from "@/features/assessments/components/assessment-result-view";

export default function AssessmentAttemptPage({
  params,
}: {
  params: Promise<{ attemptId: string }>;
}) {
  const { attemptId } = use(params);
  const { data: attempt, isPending } = useAssessmentAttempt(attemptId);
  const isFinished =
    attempt?.status === "Passed" ||
    attempt?.status === "Failed" ||
    attempt?.status === "Submitted" ||
    attempt?.status === "Grading";
  const { data: result } = useAssessmentResult(attemptId, isFinished);

  return (
    <div className="page-container page-section">
      <Button variant="ghost" size="sm" render={<Link href="/assessments" />}>
        <ArrowLeft />
        Back to assessments
      </Button>

      <div className="mt-4">
        {isPending ? (
          <div className="grid gap-4">
            <Skeleton className="h-8 w-48" />
            <Skeleton className="h-32 w-full" />
          </div>
        ) : result ? (
          <AssessmentResultView result={result} />
        ) : attempt ? (
          <AssessmentPlayer attempt={attempt} />
        ) : (
          <p className="text-sm text-muted-foreground">
            Assessment not found.
          </p>
        )}
      </div>
    </div>
  );
}
