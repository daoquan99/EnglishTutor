import { useQuery } from "@tanstack/react-query";
import { notificationsApi } from "../api/notifications-api";
import { notificationKeys } from "../api/query-keys";

export function useNotificationSettings() {
  return useQuery({
    queryKey: notificationKeys.settings(),
    queryFn: () => notificationsApi.getSettings(),
  });
}
