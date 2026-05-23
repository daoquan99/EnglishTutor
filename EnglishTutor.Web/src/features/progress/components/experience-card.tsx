"use client";

import { Star, TrendingUp } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Progress } from "@/shared/components/ui/progress";
import { Skeleton } from "@/shared/components/ui/skeleton";
import type { ExperienceInfo } from "../types/progress";
import { RANK_LABELS } from "../types/progress";

const RANK_THRESHOLDS = [
  { rank: "Beginner", min: 0 },
  { rank: "Bronze", min: 250 },
  { rank: "Silver", min: 1000 },
  { rank: "Gold", min: 2500 },
  { rank: "Platinum", min: 5000 },
  { rank: "Diamond", min: 10000 },
] as const;

function getRankProgress(totalExp: number, currentRank: string) {
  const idx = RANK_THRESHOLDS.findIndex((r) => r.rank === currentRank);
  const current = RANK_THRESHOLDS[idx] ?? RANK_THRESHOLDS[0];
  const next = RANK_THRESHOLDS[idx + 1];

  if (!next) return { percent: 100, expToNext: 0, nextRank: null };

  const range = next.min - current.min;
  const progress = totalExp - current.min;
  return {
    percent: Math.min(100, Math.round((progress / range) * 100)),
    expToNext: Math.max(0, next.min - totalExp),
    nextRank: next.rank,
  };
}

export function ExperienceCard({ data }: { data?: ExperienceInfo }) {
  if (!data) {
    return (
      <Card>
        <CardHeader>
          <Skeleton className="h-5 w-24" />
        </CardHeader>
        <CardContent>
          <Skeleton className="h-20 w-full" />
        </CardContent>
      </Card>
    );
  }

  const { percent, expToNext, nextRank } = getRankProgress(data.totalExp, data.currentAppRank);

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Star className="size-4 text-warning" />
          Experience
        </CardTitle>
      </CardHeader>
      <CardContent className="grid gap-4">
        <div className="flex items-center justify-between">
          <div>
            <p className="text-2xl font-bold tabular-nums">{data.totalExp.toLocaleString()}</p>
            <p className="text-xs text-muted-foreground">Total EXP</p>
          </div>
          <div className="rounded-lg bg-warning/10 px-3 py-1.5 text-center">
            <p className="text-sm font-semibold text-warning">
              {RANK_LABELS[data.currentAppRank] ?? data.currentAppRank}
            </p>
          </div>
        </div>
        <div className="space-y-1.5">
          <Progress value={percent} className="h-2" />
          {nextRank && (
            <div className="flex items-center gap-1 text-xs text-muted-foreground">
              <TrendingUp className="size-3" />
              {expToNext.toLocaleString()} EXP to {RANK_LABELS[nextRank]}
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
