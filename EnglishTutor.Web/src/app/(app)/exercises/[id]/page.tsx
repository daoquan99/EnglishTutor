"use client";

import { use, useState } from "react";
import Link from "next/link";
import { ArrowLeft, Loader2, Play } from "lucide-react";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { useExerciseDetail } from "@/features/exercises/hooks/use-exercise-detail";
import { useStartAttempt } from "@/features/exercises/hooks/use-start-attempt";
import { ExercisePlayer } from "@/features/exercises/components/exercise-player";

export default function ExerciseDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const { data: exercise, isPending } = useExerciseDetail(id);
  const startAttempt = useStartAttempt();
  const [attemptId, setAttemptId] = useState<string | null>(null);

  const handleStart = () => {
    startAttempt.mutate(id, {
      onSuccess: (resp) => setAttemptId(resp.attemptId),
    });
  };

  return (
    <div className="page-container page-section">
      <Button variant="ghost" size="sm" render={<Link href="/exercises" />}>
        <ArrowLeft />
        Back to exercises
      </Button>

      <div className="mt-4">
        {isPending ? (
          <div className="grid gap-4">
            <Skeleton className="h-8 w-64" />
            <Skeleton className="h-4 w-48" />
            <Skeleton className="h-40 w-full" />
          </div>
        ) : exercise ? (
          attemptId ? (
            <ExercisePlayer exercise={exercise} attemptId={attemptId} />
          ) : (
            <div className="grid gap-4">
              <h1 className="font-heading text-2xl font-bold">{exercise.title}</h1>
              {exercise.description && (
                <p className="text-sm text-muted-foreground">
                  {exercise.description}
                </p>
              )}
              <div className="flex items-center gap-2">
                <Badge variant="secondary">{exercise.level}</Badge>
                <Badge variant="outline">{exercise.skill}</Badge>
                <Badge variant="outline">{exercise.exerciseType}</Badge>
                <span className="text-sm text-muted-foreground">
                  {exercise.totalQuestions} questions
                </span>
              </div>
              <Button onClick={handleStart} disabled={startAttempt.isPending} className="w-fit">
                {startAttempt.isPending ? (
                  <Loader2 className="animate-spin" />
                ) : (
                  <Play />
                )}
                Start exercise
              </Button>
            </div>
          )
        ) : (
          <p className="text-sm text-muted-foreground">Exercise not found.</p>
        )}
      </div>
    </div>
  );
}
