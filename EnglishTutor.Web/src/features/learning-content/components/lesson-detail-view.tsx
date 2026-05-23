"use client";

import { Loader2, CheckCircle2 } from "lucide-react";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import { Separator } from "@/shared/components/ui/separator";
import { Skeleton } from "@/shared/components/ui/skeleton";
import type { LessonDetail } from "../types/learning-content";
import { useCompleteLesson } from "../hooks/use-complete-lesson";

export function LessonDetailView({ lesson }: { lesson?: LessonDetail }) {
  const completeLesson = useCompleteLesson();

  if (!lesson) {
    return (
      <div className="grid gap-4">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-4 w-48" />
        <Skeleton className="h-60 w-full" />
      </div>
    );
  }

  const sorted = [...lesson.sections].sort((a, b) => a.order - b.order);

  return (
    <div className="grid gap-6">
      <div>
        <h1 className="text-2xl font-bold">{lesson.title}</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          {lesson.description}
        </p>
        <div className="mt-2 flex items-center gap-2">
          <Badge variant="secondary">{lesson.level}</Badge>
          <Badge variant="outline">{lesson.skill}</Badge>
          <span className="text-xs text-muted-foreground">
            ~{lesson.estimatedMinutes} min
          </span>
        </div>
      </div>

      <Separator />

      {sorted.map((s) => (
        <div key={s.id}>
          <h2 className="text-lg font-semibold">{s.title}</h2>
          <div className="mt-2 prose prose-sm dark:prose-invert max-w-none whitespace-pre-wrap text-sm">
            {s.content}
          </div>
        </div>
      ))}

      <Separator />

      <Button
        onClick={() => completeLesson.mutate({ id: lesson.id })}
        disabled={completeLesson.isPending || completeLesson.isSuccess}
        className="w-fit"
      >
        {completeLesson.isPending ? (
          <Loader2 className="animate-spin" />
        ) : completeLesson.isSuccess ? (
          <CheckCircle2 />
        ) : null}
        {completeLesson.isSuccess ? "Completed!" : "Mark as complete"}
      </Button>
    </div>
  );
}
