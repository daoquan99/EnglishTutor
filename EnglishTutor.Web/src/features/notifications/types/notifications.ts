export interface Notification {
  id: string;
  type: string;
  title: string;
  body: string;
  data: string | null;
  targetUrl: string | null;
  isRead: boolean;
  channel: string;
  status: string;
  scheduledAtUtc: string;
  sentAtUtc: string | null;
  readAtUtc: string | null;
}

export interface Channels {
  inApp: boolean;
  email: boolean;
  push: boolean;
}

export interface ScheduleGroup {
  enabled: boolean;
  channels: Channels;
  time: string | null;
  beforeMinutes: number | null;
  afterMinutes: number | null;
  frequency: string | null;
  dayOfWeek: string | null;
  dayOfMonth: number | null;
}

export interface QuietHours {
  enabled: boolean;
  start: string | null;
  end: string | null;
}

export interface NotificationSettings {
  timeZone: string;
  quietHours: QuietHours;
  studyReminder: ScheduleGroup;
  missedStudyReminder: ScheduleGroup;
  mistakeReviewReminder: ScheduleGroup;
  vocabularyReviewReminder: ScheduleGroup;
  weeklySummary: ScheduleGroup;
  monthlySummary: ScheduleGroup;
  assessmentReminder: ScheduleGroup;
}

export type UpdateNotificationSettingsRequest = NotificationSettings;

export type ScheduleType = keyof Pick<
  NotificationSettings,
  | "studyReminder"
  | "missedStudyReminder"
  | "mistakeReviewReminder"
  | "vocabularyReviewReminder"
  | "weeklySummary"
  | "monthlySummary"
  | "assessmentReminder"
>;

export const DAYS_OF_WEEK = [
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
  "Sunday",
] as const;

export const FREQUENCIES = ["Daily", "Weekly", "BiWeekly"] as const;
