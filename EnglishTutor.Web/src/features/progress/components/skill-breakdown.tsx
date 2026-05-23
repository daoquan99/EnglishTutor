"use client";

import { Layers } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { cn } from "@/shared/lib/utils";
import type { SkillProgress } from "../types/progress";

const SKILL_COLORS: Record<string, { bar: string; bg: string }> = {
  Vocabulary: { bar: "bg-vocabulary", bg: "bg-vocabulary/15" },
  Grammar: { bar: "bg-exercise", bg: "bg-exercise/15" },
  Pronunciation: { bar: "bg-speaking", bg: "bg-speaking/15" },
  Speaking: { bar: "bg-speaking", bg: "bg-speaking/15" },
  Listening: { bar: "bg-study", bg: "bg-study/15" },
  Reading: { bar: "bg-info", bg: "bg-info/15" },
  Writing: { bar: "bg-exercise", bg: "bg-exercise/15" },
  Conversation: { bar: "bg-speaking", bg: "bg-speaking/15" },
};

const DEFAULT_COLOR = { bar: "bg-primary", bg: "bg-primary/15" };

export function SkillBreakdown({ data }: { data?: SkillProgress[] }) {
  if (!data) {
    return (
      <Card>
        <CardHeader>
          <Skeleton className="h-5 w-28" />
        </CardHeader>
        <CardContent>
          <div className="grid gap-3">
            {Array.from({ length: 4 }, (_, i) => (
              <Skeleton key={i} className="h-8 w-full" />
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
            <Layers className="size-4 text-assessment" />
            Skills
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">
            No skill data yet. Complete some activities to see your progress.
          </p>
        </CardContent>
      </Card>
    );
  }

  const sorted = [...data].sort((a, b) => b.score - a.score);

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Layers className="size-4 text-assessment" />
          Skills
        </CardTitle>
      </CardHeader>
      <CardContent>
        <ul className="grid gap-3">
          {sorted.map((skill) => {
            const colors = SKILL_COLORS[skill.skill] ?? DEFAULT_COLOR;
            return (
              <li key={skill.skill}>
                <div className="flex items-center justify-between text-sm">
                  <span className="font-medium">{skill.skill}</span>
                  <span className="tabular-nums text-muted-foreground">{skill.score}/100</span>
                </div>
                <div className={cn("mt-1.5 h-2 w-full overflow-hidden rounded-full", colors.bg)}>
                  <div
                    className={cn("h-full rounded-full transition-all", colors.bar)}
                    style={{ width: `${skill.score}%` }}
                  />
                </div>
              </li>
            );
          })}
        </ul>
      </CardContent>
    </Card>
  );
}
