export interface StudyPlan {
  id: string;
  userId: string;
  targetLanguageCode: string;
  preferredStudyTime: string;
  reminderBeforeMinutes: number;
  timeZoneId: string;
  dailyTargetMinutes: number;
  weeklyTargetMinutes: number;
  monthlyTargetMinutes: number;
  monthlyTargetStudyDays: number;
  isActive: boolean;
  weekDays: WeekDay[];
}

export interface WeekDay {
  dayOfWeek: number;
  isStudyDay: boolean;
}

export interface CreateStudyPlanRequest {
  targetLanguageCode: string;
  preferredStudyTime: string;
  reminderBeforeMinutes: number;
  timeZoneId: string;
  dailyTargetMinutes: number;
  weeklyTargetMinutes: number;
  monthlyTargetMinutes: number;
  monthlyTargetStudyDays: number;
  studyDays?: number[] | null;
}

export interface UpdateStudyPlanRequest {
  preferredStudyTime?: string | null;
  reminderBeforeMinutes?: number | null;
  dailyTargetMinutes?: number | null;
  weeklyTargetMinutes?: number | null;
  monthlyTargetMinutes?: number | null;
  monthlyTargetStudyDays?: number | null;
}

export interface UpdateScheduleRequest {
  days: WeekDay[];
}

export interface PlannedSession {
  id: string;
  studyPlanId: string;
  userId: string;
  targetLanguageCode: string;
  scheduledDateUtc: string;
  status: string;
  completedAtUtc: string | null;
  missedAtUtc: string | null;
}

export const DAY_LABELS = [
  "Sunday",
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
] as const;

export const SESSION_STATUS_LABELS: Record<string, string> = {
  Planned: "Planned",
  Completed: "Completed",
  Missed: "Missed",
  Skipped: "Skipped",
};
