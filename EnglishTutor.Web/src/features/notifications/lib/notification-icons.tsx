import {
  BookOpen,
  BookX,
  AlertCircle,
  Languages,
  Target,
  BarChart3,
  TrendingUp,
  ClipboardCheck,
  Star,
  Bell,
  type LucideIcon,
} from "lucide-react";

type IconConfig = {
  icon: LucideIcon;
  className: string;
  bgClassName: string;
};

const iconMap: Record<string, IconConfig> = {
  StudyReminder: {
    icon: BookOpen,
    className: "text-study",
    bgClassName: "bg-study/10",
  },
  MissedStudyReminder: {
    icon: BookX,
    className: "text-warning",
    bgClassName: "bg-warning/10",
  },
  MistakeReviewReminder: {
    icon: AlertCircle,
    className: "text-exercise",
    bgClassName: "bg-exercise/10",
  },
  VocabularyReviewReminder: {
    icon: Languages,
    className: "text-vocabulary",
    bgClassName: "bg-vocabulary/10",
  },
  DailyTargetCompleted: {
    icon: Target,
    className: "text-success",
    bgClassName: "bg-success/10",
  },
  WeeklyProgressSummary: {
    icon: BarChart3,
    className: "text-info",
    bgClassName: "bg-info/10",
  },
  MonthlyProgressSummary: {
    icon: TrendingUp,
    className: "text-info",
    bgClassName: "bg-info/10",
  },
  AssessmentReminder: {
    icon: ClipboardCheck,
    className: "text-assessment",
    bgClassName: "bg-assessment/10",
  },
  LevelUpCongratulations: {
    icon: Star,
    className: "text-primary",
    bgClassName: "bg-primary/10",
  },
};

const fallback: IconConfig = {
  icon: Bell,
  className: "text-muted-foreground",
  bgClassName: "bg-muted",
};

export function NotificationIcon({ type }: { type: string }) {
  const config = iconMap[type] ?? fallback;
  const Icon = config.icon;

  return (
    <div
      className={`flex size-8 shrink-0 items-center justify-center rounded-full ${config.bgClassName}`}
    >
      <Icon className={`size-4 ${config.className}`} />
    </div>
  );
}
