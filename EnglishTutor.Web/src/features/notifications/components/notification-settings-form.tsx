"use client";

import { useEffect, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import {
  AlertTriangle,
  Bell,
  BookOpen,
  BrainCircuit,
  Calendar,
  CalendarDays,
  CheckCircle2,
  Clock,
  GraduationCap,
  Loader2,
  Moon,
} from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { Switch } from "@/shared/components/ui/switch";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { Skeleton } from "@/shared/components/ui/skeleton";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
  CardAction,
} from "@/shared/components/ui/card";
import { cn } from "@/shared/lib/utils";
import type {
  NotificationSettings,
  ScheduleType,
  ScheduleGroup,
} from "../types/notifications";
import { DAYS_OF_WEEK, FREQUENCIES } from "../types/notifications";
import { useNotificationSettings } from "../hooks/use-notification-settings";
import { useUpdateNotificationSettings } from "../hooks/use-update-notification-settings";

const COMMON_TIMEZONES = [
  "UTC",
  "America/New_York",
  "America/Chicago",
  "America/Denver",
  "America/Los_Angeles",
  "America/Sao_Paulo",
  "Europe/London",
  "Europe/Paris",
  "Europe/Berlin",
  "Europe/Moscow",
  "Asia/Dubai",
  "Asia/Kolkata",
  "Asia/Bangkok",
  "Asia/Ho_Chi_Minh",
  "Asia/Shanghai",
  "Asia/Tokyo",
  "Asia/Seoul",
  "Asia/Singapore",
  "Australia/Sydney",
  "Pacific/Auckland",
] as const;

interface ScheduleConfig {
  key: ScheduleType;
  title: string;
  description: string;
  icon: React.ElementType;
  iconColor: string;
  showTime: boolean;
  showBeforeMinutes: boolean;
  showAfterMinutes: boolean;
  showFrequency: boolean;
  showDayOfWeek: boolean;
  showDayOfMonth: boolean;
}

const SCHEDULE_CONFIGS: ScheduleConfig[] = [
  {
    key: "studyReminder",
    title: "Study reminders",
    description: "Get reminded before your planned study session.",
    icon: Clock,
    iconColor: "bg-study/10 text-study",
    showTime: true,
    showBeforeMinutes: true,
    showAfterMinutes: false,
    showFrequency: false,
    showDayOfWeek: false,
    showDayOfMonth: false,
  },
  {
    key: "missedStudyReminder",
    title: "Missed study reminders",
    description: "Get notified when you miss a planned study session.",
    icon: AlertTriangle,
    iconColor: "bg-warning/10 text-warning",
    showTime: false,
    showBeforeMinutes: false,
    showAfterMinutes: true,
    showFrequency: false,
    showDayOfWeek: false,
    showDayOfMonth: false,
  },
  {
    key: "mistakeReviewReminder",
    title: "Mistake review reminders",
    description: "Periodic reminders to review your common mistakes.",
    icon: BrainCircuit,
    iconColor: "bg-assessment/10 text-assessment",
    showTime: true,
    showBeforeMinutes: false,
    showAfterMinutes: false,
    showFrequency: true,
    showDayOfWeek: false,
    showDayOfMonth: false,
  },
  {
    key: "vocabularyReviewReminder",
    title: "Vocabulary review reminders",
    description: "Reminders to review vocabulary using spaced repetition.",
    icon: BookOpen,
    iconColor: "bg-vocabulary/10 text-vocabulary",
    showTime: true,
    showBeforeMinutes: false,
    showAfterMinutes: false,
    showFrequency: true,
    showDayOfWeek: false,
    showDayOfMonth: false,
  },
  {
    key: "weeklySummary",
    title: "Weekly summary",
    description: "A weekly report of your learning progress.",
    icon: Calendar,
    iconColor: "bg-info/10 text-info",
    showTime: true,
    showBeforeMinutes: false,
    showAfterMinutes: false,
    showFrequency: false,
    showDayOfWeek: true,
    showDayOfMonth: false,
  },
  {
    key: "monthlySummary",
    title: "Monthly summary",
    description: "A monthly overview of your learning journey.",
    icon: CalendarDays,
    iconColor: "bg-speaking/10 text-speaking",
    showTime: true,
    showBeforeMinutes: false,
    showAfterMinutes: false,
    showFrequency: false,
    showDayOfWeek: false,
    showDayOfMonth: true,
  },
  {
    key: "assessmentReminder",
    title: "Assessment reminders",
    description: "Reminders for upcoming level assessments.",
    icon: GraduationCap,
    iconColor: "bg-exercise/10 text-exercise",
    showTime: true,
    showBeforeMinutes: false,
    showAfterMinutes: false,
    showFrequency: false,
    showDayOfWeek: false,
    showDayOfMonth: false,
  },
];

function ChannelToggles({
  prefix,
  control,
}: {
  prefix: `${ScheduleType}.channels`;
  control: ReturnType<typeof useForm<NotificationSettings>>["control"];
}) {
  const channels = [
    { name: "inApp" as const, label: "In-App" },
    { name: "email" as const, label: "Email" },
    { name: "push" as const, label: "Push" },
  ];

  return (
    <div className="flex items-center gap-4">
      {channels.map((ch) => (
        <Controller
          key={ch.name}
          control={control}
          name={`${prefix}.${ch.name}`}
          render={({ field }) => (
            <label className="flex items-center gap-1.5 text-xs">
              <Switch
                checked={!!field.value}
                onCheckedChange={field.onChange}
                className="scale-75"
              />
              <span className="text-muted-foreground">{ch.label}</span>
            </label>
          )}
        />
      ))}
    </div>
  );
}

function ScheduleCard({
  config,
  control,
  watch,
}: {
  config: ScheduleConfig;
  control: ReturnType<typeof useForm<NotificationSettings>>["control"];
  watch: ReturnType<typeof useForm<NotificationSettings>>["watch"];
}) {
  const Icon = config.icon;
  const isEnabled = watch(`${config.key}.enabled`);

  return (
    <Card size="sm">
      <CardHeader>
        <div className="flex items-center gap-2.5">
          <div
            className={cn(
              "flex size-8 shrink-0 items-center justify-center rounded-lg",
              config.iconColor,
            )}
          >
            <Icon className="size-4" />
          </div>
          <div className="min-w-0">
            <CardTitle>{config.title}</CardTitle>
            <CardDescription className="text-xs">
              {config.description}
            </CardDescription>
          </div>
        </div>
        <CardAction>
          <Controller
            control={control}
            name={`${config.key}.enabled`}
            render={({ field }) => (
              <Switch
                checked={!!field.value}
                onCheckedChange={field.onChange}
              />
            )}
          />
        </CardAction>
      </CardHeader>

      {isEnabled && (
        <CardContent className="grid gap-4">
          <div>
            <Label className="mb-1.5 text-xs text-muted-foreground">
              Delivery channels
            </Label>
            <ChannelToggles
              prefix={`${config.key}.channels`}
              control={control}
            />
          </div>

          <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
            {config.showTime && (
              <div>
                <Label className="mb-1.5 text-xs text-muted-foreground">
                  Time
                </Label>
                <Controller
                  control={control}
                  name={`${config.key}.time`}
                  render={({ field }) => (
                    <Input
                      type="time"
                      value={field.value ?? ""}
                      onChange={(e) =>
                        field.onChange(e.target.value || null)
                      }
                      className="h-8 text-sm"
                    />
                  )}
                />
              </div>
            )}

            {config.showBeforeMinutes && (
              <div>
                <Label className="mb-1.5 text-xs text-muted-foreground">
                  Remind before (min)
                </Label>
                <Controller
                  control={control}
                  name={`${config.key}.beforeMinutes`}
                  render={({ field }) => (
                    <Input
                      type="number"
                      min={0}
                      max={180}
                      placeholder="15"
                      value={field.value ?? ""}
                      onChange={(e) =>
                        field.onChange(
                          e.target.value ? Number(e.target.value) : null,
                        )
                      }
                      className="h-8 text-sm"
                    />
                  )}
                />
              </div>
            )}

            {config.showAfterMinutes && (
              <div>
                <Label className="mb-1.5 text-xs text-muted-foreground">
                  Remind after (min)
                </Label>
                <Controller
                  control={control}
                  name={`${config.key}.afterMinutes`}
                  render={({ field }) => (
                    <Input
                      type="number"
                      min={15}
                      max={1440}
                      placeholder="60"
                      value={field.value ?? ""}
                      onChange={(e) =>
                        field.onChange(
                          e.target.value ? Number(e.target.value) : null,
                        )
                      }
                      className="h-8 text-sm"
                    />
                  )}
                />
              </div>
            )}

            {config.showFrequency && (
              <div>
                <Label className="mb-1.5 text-xs text-muted-foreground">
                  Frequency
                </Label>
                <Controller
                  control={control}
                  name={`${config.key}.frequency`}
                  render={({ field }) => (
                    <Select
                      value={field.value ?? ""}
                      onValueChange={(val) => field.onChange(val || null)}
                    >
                      <SelectTrigger className="h-8 text-sm">
                        <SelectValue placeholder="Select" />
                      </SelectTrigger>
                      <SelectContent>
                        {FREQUENCIES.map((f) => (
                          <SelectItem key={f} value={f}>
                            {f === "BiWeekly" ? "Bi-Weekly" : f}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  )}
                />
              </div>
            )}

            {config.showDayOfWeek && (
              <div>
                <Label className="mb-1.5 text-xs text-muted-foreground">
                  Day of week
                </Label>
                <Controller
                  control={control}
                  name={`${config.key}.dayOfWeek`}
                  render={({ field }) => (
                    <Select
                      value={field.value ?? ""}
                      onValueChange={(val) => field.onChange(val || null)}
                    >
                      <SelectTrigger className="h-8 text-sm">
                        <SelectValue placeholder="Select" />
                      </SelectTrigger>
                      <SelectContent>
                        {DAYS_OF_WEEK.map((d) => (
                          <SelectItem key={d} value={d}>
                            {d}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  )}
                />
              </div>
            )}

            {config.showDayOfMonth && (
              <div>
                <Label className="mb-1.5 text-xs text-muted-foreground">
                  Day of month
                </Label>
                <Controller
                  control={control}
                  name={`${config.key}.dayOfMonth`}
                  render={({ field }) => (
                    <Input
                      type="number"
                      min={1}
                      max={28}
                      placeholder="1"
                      value={field.value ?? ""}
                      onChange={(e) =>
                        field.onChange(
                          e.target.value ? Number(e.target.value) : null,
                        )
                      }
                      className="h-8 text-sm"
                    />
                  )}
                />
              </div>
            )}
          </div>
        </CardContent>
      )}
    </Card>
  );
}

function defaultSchedule(): ScheduleGroup {
  return {
    enabled: false,
    channels: { inApp: true, email: false, push: false },
    time: null,
    beforeMinutes: null,
    afterMinutes: null,
    frequency: null,
    dayOfWeek: null,
    dayOfMonth: null,
  };
}

function settingsToForm(s: NotificationSettings): NotificationSettings {
  return {
    timeZone: s.timeZone,
    quietHours: { ...s.quietHours },
    studyReminder: { ...s.studyReminder },
    missedStudyReminder: { ...s.missedStudyReminder },
    mistakeReviewReminder: { ...s.mistakeReviewReminder },
    vocabularyReviewReminder: { ...s.vocabularyReviewReminder },
    weeklySummary: { ...s.weeklySummary },
    monthlySummary: { ...s.monthlySummary },
    assessmentReminder: { ...s.assessmentReminder },
  };
}

export function NotificationSettingsForm() {
  const { data: settings, isPending } = useNotificationSettings();
  const updateSettings = useUpdateNotificationSettings();
  const [showSuccess, setShowSuccess] = useState(false);

  const { control, handleSubmit, reset, watch, formState } =
    useForm<NotificationSettings>({
      defaultValues: {
        timeZone: "UTC",
        quietHours: { enabled: false, start: null, end: null },
        studyReminder: defaultSchedule(),
        missedStudyReminder: defaultSchedule(),
        mistakeReviewReminder: defaultSchedule(),
        vocabularyReviewReminder: defaultSchedule(),
        weeklySummary: defaultSchedule(),
        monthlySummary: defaultSchedule(),
        assessmentReminder: defaultSchedule(),
      },
    });

  useEffect(() => {
    if (settings) {
      reset(settingsToForm(settings));
    }
  }, [settings, reset]);

  if (isPending) {
    return (
      <div className="grid gap-3">
        {Array.from({ length: 4 }, (_, i) => (
          <Skeleton key={i} className="h-24 w-full rounded-xl" />
        ))}
      </div>
    );
  }

  const onSubmit = (data: NotificationSettings) => {
    updateSettings.mutate(data, {
      onSuccess: () => {
        setShowSuccess(true);
        setTimeout(() => setShowSuccess(false), 2500);
      },
    });
  };

  const quietHoursEnabled = watch("quietHours.enabled");

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="grid gap-4">
      {/* Timezone */}
      <Card size="sm">
        <CardHeader>
          <div className="flex items-center gap-2.5">
            <div className="flex size-8 shrink-0 items-center justify-center rounded-lg bg-primary/10">
              <Bell className="size-4 text-primary" />
            </div>
            <div>
              <CardTitle>Timezone</CardTitle>
              <CardDescription className="text-xs">
                All notification times are based on this timezone.
              </CardDescription>
            </div>
          </div>
          <CardAction>
            <Controller
              control={control}
              name="timeZone"
              render={({ field }) => (
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger className="h-8 w-52 text-sm">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {COMMON_TIMEZONES.map((tz) => (
                      <SelectItem key={tz} value={tz}>
                        {tz.replace(/_/g, " ")}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
          </CardAction>
        </CardHeader>
      </Card>

      {/* Quiet hours */}
      <Card size="sm">
        <CardHeader>
          <div className="flex items-center gap-2.5">
            <div className="flex size-8 shrink-0 items-center justify-center rounded-lg bg-muted">
              <Moon className="size-4 text-muted-foreground" />
            </div>
            <div>
              <CardTitle>Quiet hours</CardTitle>
              <CardDescription className="text-xs">
                Suppress notifications during these hours.
              </CardDescription>
            </div>
          </div>
          <CardAction>
            <Controller
              control={control}
              name="quietHours.enabled"
              render={({ field }) => (
                <Switch
                  checked={!!field.value}
                  onCheckedChange={field.onChange}
                />
              )}
            />
          </CardAction>
        </CardHeader>
        {quietHoursEnabled && (
          <CardContent>
            <div className="flex items-center gap-3">
              <div>
                <Label className="mb-1.5 text-xs text-muted-foreground">
                  Start
                </Label>
                <Controller
                  control={control}
                  name="quietHours.start"
                  render={({ field }) => (
                    <Input
                      type="time"
                      value={field.value ?? ""}
                      onChange={(e) =>
                        field.onChange(e.target.value || null)
                      }
                      className="h-8 w-28 text-sm"
                    />
                  )}
                />
              </div>
              <span className="mt-5 text-sm text-muted-foreground">to</span>
              <div>
                <Label className="mb-1.5 text-xs text-muted-foreground">
                  End
                </Label>
                <Controller
                  control={control}
                  name="quietHours.end"
                  render={({ field }) => (
                    <Input
                      type="time"
                      value={field.value ?? ""}
                      onChange={(e) =>
                        field.onChange(e.target.value || null)
                      }
                      className="h-8 w-28 text-sm"
                    />
                  )}
                />
              </div>
            </div>
          </CardContent>
        )}
      </Card>

      {/* Per-type schedule cards */}
      {SCHEDULE_CONFIGS.map((config) => (
        <ScheduleCard
          key={config.key}
          config={config}
          control={control}
          watch={watch}
        />
      ))}

      {/* Save button */}
      <div className="flex items-center gap-3 pt-2">
        <Button
          type="submit"
          disabled={!formState.isDirty || updateSettings.isPending}
        >
          {updateSettings.isPending ? (
            <Loader2 className="animate-spin" />
          ) : showSuccess ? (
            <CheckCircle2 />
          ) : null}
          {showSuccess ? "Saved!" : "Save settings"}
        </Button>
        {updateSettings.isError && (
          <p className="text-sm text-destructive">
            Failed to save. Please try again.
          </p>
        )}
      </div>
    </form>
  );
}
