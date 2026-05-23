"use client";

import Link from "next/link";
import { Calendar, Sparkles, Trophy, Zap, Activity } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { PageHeader } from "@/shared/components/page-header";
import { useActiveLanguage } from "@/shared/hooks/use-active-language";
import { useExperience } from "@/features/progress/hooks/use-experience";
import { useSkillProgress } from "@/features/progress/hooks/use-skill-progress";
import { useActivities } from "@/features/progress/hooks/use-activities";
import { useWeeklyProgress } from "@/features/progress/hooks/use-weekly-progress";
import { useMonthlyProgress } from "@/features/progress/hooks/use-monthly-progress";
import { ExperienceCard } from "@/features/progress/components/experience-card";
import { SkillBreakdown } from "@/features/progress/components/skill-breakdown";
import { ActivityFeed } from "@/features/progress/components/activity-feed";
import { WeeklyChart } from "@/features/progress/components/weekly-chart";

function MonthlyCard({
  data,
}: {
  data?: { expEarned: number; activityCount: number; year: number; month: number };
}) {
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

  const monthName = new Date(data.year, data.month - 1).toLocaleString("default", {
    month: "long",
  });

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="flex items-center gap-2">
            <Calendar className="size-4 text-assessment" />
            This Month
          </CardTitle>
          <span className="text-xs text-muted-foreground">
            {monthName} {data.year}
          </span>
        </div>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 gap-4">
          <div className="rounded-lg bg-assessment/10 p-3">
            <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
              <Zap className="size-3 text-assessment" />
              EXP Earned
            </div>
            <p className="mt-1 text-2xl font-bold tabular-nums">{data.expEarned.toLocaleString()}</p>
          </div>
          <div className="rounded-lg bg-study/10 p-3">
            <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
              <Activity className="size-3 text-study" />
              Activities
            </div>
            <p className="mt-1 text-2xl font-bold tabular-nums">{data.activityCount}</p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

export default function ProgressPage() {
  const lang = useActiveLanguage();
  const { data: experience } = useExperience(lang);
  const { data: skills } = useSkillProgress(lang);
  const { data: activities } = useActivities(lang);
  const { data: weekly } = useWeeklyProgress(lang);
  const { data: monthly } = useMonthlyProgress(lang);

  if (!lang) {
    return (
      <div className="page-container page-section">
        <div className="mx-auto max-w-md py-12 text-center">
          <div className="mx-auto mb-4 flex size-12 items-center justify-center rounded-full bg-primary/10">
            <Sparkles className="size-6 text-primary" />
          </div>
          <h2 className="font-heading text-lg font-semibold">No language selected</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            Set your target language in{" "}
            <Link href="/settings" className="text-primary underline-offset-4 hover:underline">
              Settings
            </Link>{" "}
            first.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="page-container page-section">
      <PageHeader
        icon={Trophy}
        iconColor="bg-warning/10 text-warning"
        title="Progress"
        description="Track your learning journey and skill development."
      />

      <div className="mt-6 grid gap-4">
        <ExperienceCard data={experience} />

        <div className="grid gap-4 md:grid-cols-2">
          <WeeklyChart data={weekly} />
          <MonthlyCard data={monthly} />
        </div>

        <SkillBreakdown data={skills} />
        <ActivityFeed data={activities} />
      </div>
    </div>
  );
}
