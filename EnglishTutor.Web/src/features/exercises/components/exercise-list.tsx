"use client";

import Link from "next/link";
import { Badge } from "@/shared/components/ui/badge";
import { Skeleton } from "@/shared/components/ui/skeleton";
import type { ExerciseListItem } from "../types/exercises";

export function ExerciseList({ exercises }: { exercises?: ExerciseListItem[] }) {
  if (!exercises) {
    return (
      <div className="grid gap-2">
        {Array.from({ length: 5 }, (_, i) => (
          <Skeleton key={i} className="h-20 w-full rounded-lg" />
        ))}
      </div>
    );
  }

  if (!exercises.length) {
    return (
      <p className="py-8 text-center text-sm text-muted-foreground">
        No exercises found for the selected filters.
      </p>
    );
  }

  return (
    <ul className="grid gap-2">
      {exercises.map((ex) => (
        <li key={ex.id}>
          <Link
            href={`/exercises/${ex.id}`}
            className="flex items-center justify-between rounded-lg border p-4 transition-colors hover:bg-muted/50"
          >
            <div>
              <p className="font-medium">{ex.title}</p>
              <p className="mt-0.5 text-xs text-muted-foreground">
                {ex.topic} &middot; {ex.totalQuestions} questions
              </p>
            </div>
            <div className="flex items-center gap-2">
              <Badge variant="secondary">{ex.level}</Badge>
              <Badge variant="outline">{ex.skill}</Badge>
            </div>
          </Link>
        </li>
      ))}
    </ul>
  );
}
