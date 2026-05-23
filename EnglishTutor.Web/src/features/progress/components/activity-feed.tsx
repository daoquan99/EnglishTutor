"use client";

import { Clock, History, Zap } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Skeleton } from "@/shared/components/ui/skeleton";
import type { ActivityLog } from "../types/progress";
import { ACTIVITY_TYPE_LABELS } from "../types/progress";

function formatDuration(seconds: number): string {
  if (seconds < 60) return `${seconds}s`;
  const mins = Math.floor(seconds / 60);
  return `${mins}m`;
}

function formatTime(utc: string): string {
  return new Date(utc).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
}

export function ActivityFeed({ data }: { data?: ActivityLog[] }) {
  if (!data) {
    return (
      <Card>
        <CardHeader>
          <Skeleton className="h-5 w-32" />
        </CardHeader>
        <CardContent>
          <div className="grid gap-3">
            {Array.from({ length: 3 }, (_, i) => (
              <Skeleton key={i} className="h-12 w-full" />
            ))}
          </div>
        </CardContent>
      </Card>
    );
  }

  if (!data.length) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <History className="size-4 text-muted-foreground" />
            Recent Activity
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">No recent activity.</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <History className="size-4 text-muted-foreground" />
          Recent Activity
        </CardTitle>
      </CardHeader>
      <CardContent>
        <ul className="relative grid gap-0.5">
          <div className="absolute bottom-3 left-[11px] top-3 w-px bg-border" />
          {data.slice(0, 8).map((activity) => (
            <li
              key={activity.activityId}
              className="relative flex items-start gap-3 py-2"
            >
              <div className="relative z-10 mt-0.5 flex size-[22px] shrink-0 items-center justify-center rounded-full border bg-background">
                <div className="size-2 rounded-full bg-primary" />
              </div>
              <div className="flex flex-1 items-center justify-between gap-2">
                <div className="min-w-0">
                  <p className="text-sm font-medium leading-tight">
                    {ACTIVITY_TYPE_LABELS[activity.activityType] ?? activity.activityType}
                  </p>
                  {activity.result && (
                    <p className="mt-0.5 truncate text-xs text-muted-foreground">
                      {activity.result}
                    </p>
                  )}
                </div>
                <div className="flex shrink-0 items-center gap-2 text-xs text-muted-foreground">
                  {activity.expEarned > 0 && (
                    <span className="flex items-center gap-0.5 font-medium text-study">
                      <Zap className="size-3" />
                      +{activity.expEarned}
                    </span>
                  )}
                  {activity.durationSeconds > 0 && (
                    <span className="flex items-center gap-0.5">
                      <Clock className="size-3" />
                      {formatDuration(activity.durationSeconds)}
                    </span>
                  )}
                  <span className="tabular-nums">{formatTime(activity.completedAtUtc)}</span>
                </div>
              </div>
            </li>
          ))}
        </ul>
      </CardContent>
    </Card>
  );
}
