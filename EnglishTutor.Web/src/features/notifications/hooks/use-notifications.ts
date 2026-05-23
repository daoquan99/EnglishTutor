import { useQuery } from "@tanstack/react-query";
import { notificationsApi } from "../api/notifications-api";
import { notificationKeys } from "../api/query-keys";

export function useNotifications(params?: { isRead?: boolean }) {
  return useQuery({
    queryKey: notificationKeys.list(params),
    queryFn: () => notificationsApi.list(params),
    refetchInterval: 60_000,
  });
}
