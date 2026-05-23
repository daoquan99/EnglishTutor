"use client";

import Link from "next/link";
import { BookOpen, BrainCircuit, Mic, Sparkles } from "lucide-react";
import { useAuthStore } from "@/features/auth/hooks/use-auth-store";
import { useActiveLanguage } from "@/shared/hooks/use-active-language";
import { useDashboard } from "@/features/progress/hooks/use-dashboard";
import { useExperience } from "@/features/progress/hooks/use-experience";
import { useSkillProgress } from "@/features/progress/hooks/use-skill-progress";
import { useActivities } from "@/features/progress/hooks/use-activities";
import { useWeeklyProgress } from "@/features/progress/hooks/use-weekly-progress";
import { LevelBadge } from "@/features/users/components/level-badge";
import { TodaySnapshotCard } from "@/features/progress/components/today-snapshot-card";
import { ExperienceCard } from "@/features/progress/components/experience-card";
import { SkillBreakdown } from "@/features/progress/components/skill-breakdown";
import { ActivityFeed } from "@/features/progress/components/activity-feed";
import { WeeklyChart } from "@/features/progress/components/weekly-chart";

const quickActions = [
  { href: "/speaking", label: "Start Speaking", icon: Mic, color: "bg-speaking/10 text-speaking hover:bg-speaking/15" },
  { href: "/vocabulary", label: "Review Vocab", icon: BookOpen, color: "bg-vocabulary/10 text-vocabulary hover:bg-vocabulary/15" },
  { href: "/exercises", label: "Practice", icon: BrainCircuit, color: "bg-exercise/10 text-exercise hover:bg-exercise/15" },
];

export default function DashboardPage() {
  const user = useAuthStore((s) => s.user);
  const lang = useActiveLanguage();
  const { data: dashboard } = useDashboard(lang);
  const { data: experience } = useExperience(lang);
  const { data: skills } = useSkillProgress(lang);
  const { data: activities } = useActivities(lang);
  const { data: weekly } = useWeeklyProgress(lang);

  if (!lang) {
    return (
      <div className="page-container page-section">
        <div className="mx-auto max-w-md py-12 text-center">
          <div className="mx-auto mb-4 flex size-12 items-center justify-center rounded-full bg-primary/10">
            <Sparkles className="size-6 text-primary" />
          </div>
          <h1 className="font-heading text-xl font-bold">Welcome to EnglishTutor</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            Set your target language in{" "}
            <Link href="/settings" className="text-primary underline-offset-4 hover:underline">
              Settings
            </Link>{" "}
            to get started with your learning journey.
          </p>
        </div>
      </div>
    );
  }

  const firstName = user?.displayName.split(" ")[0] ?? "Learner";

  return (
    <div className="page-container page-section">
      {/* Welcome header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="font-heading text-2xl font-bold tracking-tight">
            Welcome back, {firstName}
          </h1>
          <div className="mt-1 flex items-center gap-2 text-sm text-muted-foreground">
            <span>Keep up the great work!</span>
            {dashboard?.currentLevel && <LevelBadge level={dashboard.currentLevel} />}
          </div>
        </div>

        {/* Quick actions */}
        <div className="flex gap-2">
          {quickActions.map(({ href, label, icon: Icon, color }) => (
            <Link
              key={href}
              href={href}
              className={`inline-flex items-center gap-1.5 rounded-full px-3 py-1.5 text-xs font-medium transition-colors ${color}`}
            >
              <Icon className="size-3.5" />
              {label}
            </Link>
          ))}
        </div>
      </div>

      {/* Stats grid */}
      <div className="mt-6">
        <TodaySnapshotCard data={dashboard} />
      </div>

      {/* Experience + Weekly */}
      <div className="mt-4 grid gap-4 md:grid-cols-2">
        <ExperienceCard data={experience} />
        <WeeklyChart data={weekly} />
      </div>

      {/* Skills + Activity */}
      <div className="mt-4 grid gap-4 md:grid-cols-2">
        <SkillBreakdown data={skills} />
        <ActivityFeed data={activities} />
      </div>
    </div>
  );
}
