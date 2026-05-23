"use client";

import Link from "next/link";
import { Badge } from "@/shared/components/ui/badge";
import { Skeleton } from "@/shared/components/ui/skeleton";
import type { LessonListItem } from "../types/learning-content";

export function LessonList({ lessons }: { lessons?: LessonListItem[] }) {
  if (!lessons) {
    return (
      <div className="grid gap-2">
        {Array.from({ length: 5 }, (_, i) => (
          <Skeleton key={i} className="h-20 w-full rounded-lg" />
        ))}
      </div>
    );
  }

  if (!lessons.length) {
    return (
      <p className="py-8 text-center text-sm text-muted-foreground">
        No lessons available.
      </p>
    );
  }

  return (
    <ul className="grid gap-2">
      {lessons.map((l) => (
        <li key={l.id}>
          <Link
            href={`/lessons/${l.id}`}
            className="flex items-center justify-between rounded-lg border p-4 transition-colors hover:bg-muted/50"
          >
            <div>
              <p className="font-medium">{l.title}</p>
              <p className="mt-0.5 text-xs text-muted-foreground">
                {l.topic} &middot; {l.skill} &middot; ~{l.estimatedMinutes} min
              </p>
            </div>
            <Badge variant="secondary">{l.level}</Badge>
          </Link>
        </li>
      ))}
    </ul>
  );
}
