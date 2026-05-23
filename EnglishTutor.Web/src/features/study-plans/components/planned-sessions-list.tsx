"use client";

import { Loader2, SkipForward } from "lucide-react";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import { Skeleton } from "@/shared/components/ui/skeleton";
import type { PlannedSession } from "../types/study-plans";
import { SESSION_STATUS_LABELS } from "../types/study-plans";
import { useSkipSession } from "../hooks/use-skip-session";

function formatDate(utc: string) {
  return new Date(utc).toLocaleDateString(undefined, {
    weekday: "short",
    month: "short",
    day: "numeric",
  });
}

export function PlannedSessionsList({
  sessions,
}: {
  sessions?: PlannedSession[];
}) {
  const skipSession = useSkipSession();

  if (!sessions) {
    return (
      <div className="grid gap-2">
        {Array.from({ length: 5 }, (_, i) => (
          <Skeleton key={i} className="h-14 w-full rounded-lg" />
        ))}
      </div>
    );
  }

  if (!sessions.length) {
    return (
      <p className="py-4 text-center text-sm text-muted-foreground">
        No planned sessions.
      </p>
    );
  }

  return (
    <ul className="grid gap-2">
      {sessions.map((s) => (
        <li
          key={s.id}
          className="flex items-center justify-between rounded-lg border p-3"
        >
          <div className="flex items-center gap-3">
            <span className="text-sm">{formatDate(s.scheduledDateUtc)}</span>
            <Badge
              variant={s.status === "Completed" ? "default" : "secondary"}
            >
              {SESSION_STATUS_LABELS[s.status] ?? s.status}
            </Badge>
          </div>
          {s.status === "Planned" && (
            <Button
              variant="ghost"
              size="sm"
              onClick={() => skipSession.mutate(s.id)}
              disabled={skipSession.isPending}
            >
              {skipSession.isPending ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                <SkipForward className="h-4 w-4" />
              )}
              Skip
            </Button>
          )}
        </li>
      ))}
    </ul>
  );
}
