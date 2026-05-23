"use client";

import {
  BookOpen,
  Brain,
  Flame,
  Mic,
  Trophy,
  Zap,
} from "lucide-react";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Skeleton } from "@/shared/components/ui/skeleton";
import type { DashboardSnapshot } from "../types/progress";

const stats = [
  { key: "streakDays", icon: Flame, label: "Day Streak", bg: "bg-warning/10", text: "text-warning" },
  { key: "totalExp", icon: Zap, label: "Total EXP", bg: "bg-study/10", text: "text-study" },
  { key: "vocabularyMastered", icon: BookOpen, label: "Vocab Mastered", bg: "bg-vocabulary/10", text: "text-vocabulary" },
  { key: "totalSpeakingSessions", icon: Mic, label: "Speaking Sessions", bg: "bg-speaking/10", text: "text-speaking" },
  { key: "totalExercisesCompleted", icon: Brain, label: "Exercises Done", bg: "bg-exercise/10", text: "text-exercise" },
  { key: "totalMistakes", icon: Trophy, label: "Mistakes Tracked", bg: "bg-assessment/10", text: "text-assessment" },
] as const;

export function TodaySnapshotCard({ data }: { data?: DashboardSnapshot }) {
  if (!data) {
    return (
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
        {Array.from({ length: 6 }, (_, i) => (
          <Skeleton key={i} className="h-24 w-full rounded-xl" />
        ))}
      </div>
    );
  }

  return (
    <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
      {stats.map(({ key, icon: Icon, label, bg, text }) => (
        <Card key={key} className="border-0 shadow-soft">
          <CardContent className="flex flex-col items-center gap-2 py-4 text-center">
            <div className={`flex size-9 items-center justify-center rounded-lg ${bg}`}>
              <Icon className={`size-4.5 ${text}`} />
            </div>
            <div>
              <p className="text-xl font-bold tabular-nums leading-tight">
                {(data[key] as number).toLocaleString()}
              </p>
              <p className="mt-0.5 text-[11px] text-muted-foreground">{label}</p>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
