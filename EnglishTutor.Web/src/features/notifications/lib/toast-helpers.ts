import { toast } from "sonner";

export type NotificationPushPayload = {
  id: string;
  type: string;
  title: string;
  body: string;
  scheduledAtUtc: string;
};

const typeToastMap: Record<string, "success" | "info" | "warning"> = {
  DailyTargetCompleted: "success",
  LevelUpCongratulations: "success",
  WeeklyProgressSummary: "info",
  MonthlyProgressSummary: "info",
  StudyReminder: "info",
  AssessmentReminder: "info",
  MissedStudyReminder: "warning",
  MistakeReviewReminder: "warning",
  VocabularyReviewReminder: "info",
};

export function showNotificationToast(notification: NotificationPushPayload) {
  const toastType = typeToastMap[notification.type] ?? "info";

  toast[toastType](notification.title, {
    description: notification.body,
    duration: 6000,
  });
}
