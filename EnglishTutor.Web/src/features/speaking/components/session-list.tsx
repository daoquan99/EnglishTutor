"use client";

import Link from "next/link";
import { Badge } from "@/shared/components/ui/badge";
import { Skeleton } from "@/shared/components/ui/skeleton";
import type { SpeakingSession } from "../types/speaking";
import { SESSION_STATUS_LABELS } from "../types/speaking";

function formatDate(utc: string) {
  return new Date(utc).toLocaleDateString(undefined, {
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function SessionList({ sessions }: { sessions?: SpeakingSession[] }) {
  if (!sessions) {
    return (
      <div className="grid gap-2">
        {Array.from({ length: 3 }, (_, i) => (
          <Skeleton key={i} className="h-16 w-full rounded-lg" />
        ))}
      </div>
    );
  }

  if (!sessions.length) {
    return (
      <p className="py-8 text-center text-sm text-muted-foreground">
        No speaking sessions yet. Start one!
      </p>
    );
  }

  return (
    <ul className="grid gap-2">
      {sessions.map((s) => (
        <li key={s.sessionId}>
          <Link
            href={`/speaking/${s.sessionId}`}
            className="flex items-center justify-between rounded-lg border p-3 transition-colors hover:bg-muted/50"
          >
            <div>
              <p className="font-medium">{s.sessionType}</p>
              <p className="text-xs text-muted-foreground">
                {s.topic ?? "Free conversation"} &middot; {formatDate(s.startedAtUtc)}
              </p>
            </div>
            <Badge variant={s.status === "Completed" ? "default" : "secondary"}>
              {SESSION_STATUS_LABELS[s.status] ?? s.status}
            </Badge>
          </Link>
        </li>
      ))}
    </ul>
  );
}
