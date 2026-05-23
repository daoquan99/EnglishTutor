"use client";

import { BarChart3, Zap, Activity } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Skeleton } from "@/shared/components/ui/skeleton";
import type { WeeklyProgress } from "../types/progress";

export function WeeklyChart({ data }: { data?: WeeklyProgress }) {
  if (!data) {
    return (
      <Card>
        <CardHeader>
          <Skeleton className="h-5 w-28" />
        </CardHeader>
        <CardContent>
          <Skeleton className="h-20 w-full" />
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="flex items-center gap-2">
            <BarChart3 className="size-4 text-info" />
            This Week
          </CardTitle>
          <span className="text-xs text-muted-foreground">
            Week {data.weekNumber}, {data.year}
          </span>
        </div>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 gap-4">
          <div className="rounded-lg bg-study/10 p-3">
            <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
              <Zap className="size-3 text-study" />
              EXP Earned
            </div>
            <p className="mt-1 text-2xl font-bold tabular-nums">{data.expEarned.toLocaleString()}</p>
          </div>
          <div className="rounded-lg bg-info/10 p-3">
            <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
              <Activity className="size-3 text-info" />
              Activities
            </div>
            <p className="mt-1 text-2xl font-bold tabular-nums">{data.activityCount}</p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
