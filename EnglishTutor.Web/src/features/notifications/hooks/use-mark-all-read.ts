import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { notificationsApi } from "../api/notifications-api";
import { notificationKeys } from "../api/query-keys";

export function useMarkAllRead() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => notificationsApi.markAllRead(),
    onSuccess: (count) => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all });
      if (count > 0) {
        toast.success(`Marked ${count} notification${count === 1 ? "" : "s"} as read`);
      }
    },
  });
}
