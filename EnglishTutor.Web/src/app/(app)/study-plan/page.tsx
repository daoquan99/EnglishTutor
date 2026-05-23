"use client";

import Link from "next/link";
import { NotebookPen, Sparkles } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Separator } from "@/shared/components/ui/separator";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { PageHeader } from "@/shared/components/page-header";
import { useActiveLanguage } from "@/shared/hooks/use-active-language";
import { useStudyPlan } from "@/features/study-plans/hooks/use-study-plan";
import { usePlannedSessions } from "@/features/study-plans/hooks/use-planned-sessions";
import { StudyPlanForm } from "@/features/study-plans/components/study-plan-form";
import { WeekScheduleEditor } from "@/features/study-plans/components/week-schedule-editor";
import { PlannedSessionsList } from "@/features/study-plans/components/planned-sessions-list";

export default function StudyPlanPage() {
  const lang = useActiveLanguage();
  const { data: plan, isPending } = useStudyPlan(lang ?? undefined);
  const { data: sessions } = usePlannedSessions();

  if (!lang) {
    return (
      <div className="page-container page-section">
        <div className="mx-auto max-w-md py-12 text-center">
          <div className="mx-auto mb-4 flex size-12 items-center justify-center rounded-full bg-primary/10">
            <Sparkles className="size-6 text-primary" />
          </div>
          <h2 className="font-heading text-lg font-semibold">No language selected</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            Set an active target language in{" "}
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
        icon={NotebookPen}
        iconColor="bg-study/10 text-study"
        title="Study Plan"
        description="Manage your study schedule and goals."
      />

      {isPending ? (
        <div className="mt-6 grid gap-4">
          <Skeleton className="h-40 w-full rounded-xl" />
          <Skeleton className="h-60 w-full rounded-xl" />
        </div>
      ) : plan ? (
        <div className="mt-6 grid gap-6 lg:grid-cols-2">
          <Card>
            <CardHeader>
              <CardTitle>Goals</CardTitle>
            </CardHeader>
            <CardContent>
              <StudyPlanForm plan={plan} />
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Weekly Schedule</CardTitle>
            </CardHeader>
            <CardContent>
              <WeekScheduleEditor
                weekDays={plan.weekDays}
                targetLanguageCode={plan.targetLanguageCode}
              />
            </CardContent>
          </Card>

          <div className="lg:col-span-2">
            <Separator className="mb-6" />
            <h2 className="font-heading mb-4 text-lg font-semibold">Planned Sessions</h2>
            <PlannedSessionsList sessions={sessions} />
          </div>
        </div>
      ) : (
        <div className="mx-auto mt-12 max-w-sm text-center">
          <p className="text-sm text-muted-foreground">
            No study plan found. Create one to get started.
          </p>
        </div>
      )}
    </div>
  );
}
