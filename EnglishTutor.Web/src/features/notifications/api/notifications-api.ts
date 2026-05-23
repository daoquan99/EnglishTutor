import { httpClient } from "@/shared/api";
import type {
  Notification,
  NotificationSettings,
  UpdateNotificationSettingsRequest,
} from "../types/notifications";

export const notificationsApi = {
  list: (params?: { page?: number; pageSize?: number; isRead?: boolean }) =>
    httpClient.get<Notification[]>("/api/notifications", {
      params: params as
        | Record<string, string | number | boolean | undefined>
        | undefined,
    }),

  markRead: (id: string) =>
    httpClient.post<Notification>(`/api/notifications/${id}/mark-read`),

  markAllRead: () =>
    httpClient.post<number>("/api/notifications/mark-all-read"),

  getSettings: () =>
    httpClient.get<NotificationSettings>("/api/notifications/settings"),

  updateSettings: (data: UpdateNotificationSettingsRequest) =>
    httpClient.put<NotificationSettings>("/api/notifications/settings", data),
};
