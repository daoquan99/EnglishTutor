"use client";

import { Loader2 } from "lucide-react";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import { Skeleton } from "@/shared/components/ui/skeleton";
import type { Mistake } from "../types/mistakes";
import { MISTAKE_STATUS_LABELS } from "../types/mistakes";
import { useReviewMistake } from "../hooks/use-review-mistake";
import { useMarkMistakeMastered } from "../hooks/use-mark-mistake-mastered";

export function MistakeList({ mistakes }: { mistakes?: Mistake[] }) {
  const reviewMistake = useReviewMistake();
  const markMastered = useMarkMistakeMastered();

  if (!mistakes) {
    return (
      <div className="grid gap-2">
        {Array.from({ length: 5 }, (_, i) => (
          <Skeleton key={i} className="h-24 w-full rounded-lg" />
        ))}
      </div>
    );
  }

  if (!mistakes.length) {
    return (
      <p className="py-8 text-center text-sm text-muted-foreground">
        No mistakes to review. Great job!
      </p>
    );
  }

  return (
    <ul className="grid gap-3">
      {mistakes.map((m) => (
        <li key={m.id} className="rounded-lg border p-4">
          <div className="flex items-start justify-between">
            <div className="grid gap-1">
              <div className="flex items-center gap-2">
                <Badge variant="secondary">{m.category}</Badge>
                <Badge variant="outline">{m.type}</Badge>
                <Badge
                  variant={
                    m.status === "Mastered" ? "default" : "secondary"
                  }
                >
                  {MISTAKE_STATUS_LABELS[m.status] ?? m.status}
                </Badge>
              </div>
              <p className="text-sm line-through text-red-500">
                {m.originalText}
              </p>
              <p className="text-sm text-green-600 dark:text-green-400">
                {m.correctedText}
              </p>
              <p className="text-xs text-muted-foreground">{m.explanation}</p>
            </div>
            {m.status !== "Mastered" && (
              <div className="flex gap-1">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => reviewMistake.mutate(m.id)}
                  disabled={reviewMistake.isPending}
                >
                  {reviewMistake.isPending && (
                    <Loader2 className="h-3 w-3 animate-spin" />
                  )}
                  Review
                </Button>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => markMastered.mutate(m.id)}
                  disabled={markMastered.isPending}
                >
                  {markMastered.isPending && (
                    <Loader2 className="h-3 w-3 animate-spin" />
                  )}
                  Mastered
                </Button>
              </div>
            )}
          </div>
        </li>
      ))}
    </ul>
  );
}
