import { useMutation, useQueryClient } from "@tanstack/react-query";
import { notificationsApi } from "../api/notifications-api";
import { notificationKeys } from "../api/query-keys";

export function useMarkNotificationRead() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => notificationsApi.markRead(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all });
    },
  });
}
