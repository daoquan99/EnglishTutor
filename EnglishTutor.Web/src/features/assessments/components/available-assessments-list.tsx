"use client";

import { Loader2 } from "lucide-react";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import { Skeleton } from "@/shared/components/ui/skeleton";
import type { AvailableAssessment } from "../types/assessments";
import { useStartLevelUp } from "../hooks/use-start-level-up";

interface AvailableAssessmentsListProps {
  assessments?: AvailableAssessment[];
  targetLanguageCode?: string;
}

export function AvailableAssessmentsList({
  assessments,
  targetLanguageCode,
}: AvailableAssessmentsListProps) {
  const startLevelUp = useStartLevelUp();

  if (!assessments) {
    return (
      <div className="grid gap-2">
        {Array.from({ length: 3 }, (_, i) => (
          <Skeleton key={i} className="h-24 w-full rounded-lg" />
        ))}
      </div>
    );
  }

  if (!assessments.length) {
    return (
      <p className="py-8 text-center text-sm text-muted-foreground">
        No assessments available right now.
      </p>
    );
  }

  return (
    <ul className="grid gap-3">
      {assessments.map((a) => (
        <li
          key={a.id}
          className="flex items-center justify-between rounded-lg border p-4"
        >
          <div>
            <p className="font-medium">{a.title}</p>
            {a.description && (
              <p className="mt-0.5 text-xs text-muted-foreground">
                {a.description}
              </p>
            )}
            <div className="mt-1.5 flex items-center gap-2 text-xs text-muted-foreground">
              <Badge variant="secondary" className="text-xs">
                {a.forLevel}
              </Badge>
              <span>Pass: {a.passingScore}%</span>
              {a.timeLimitMinutes && <span>{a.timeLimitMinutes} min</span>}
            </div>
          </div>
          <Button
            size="sm"
            disabled={startLevelUp.isPending}
            onClick={() =>
              startLevelUp.mutate({
                targetLanguageCode: targetLanguageCode ?? null,
              })
            }
          >
            {startLevelUp.isPending && <Loader2 className="animate-spin" />}
            Start
          </Button>
        </li>
      ))}
    </ul>
  );
}
