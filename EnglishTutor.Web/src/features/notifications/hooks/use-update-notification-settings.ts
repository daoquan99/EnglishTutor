import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { notificationsApi } from "../api/notifications-api";
import { notificationKeys } from "../api/query-keys";
import type { UpdateNotificationSettingsRequest } from "../types/notifications";

export function useUpdateNotificationSettings() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: UpdateNotificationSettingsRequest) =>
      notificationsApi.updateSettings(data),
    onSuccess: (settings) => {
      queryClient.setQueryData(notificationKeys.settings(), settings);
      toast.success("Notification settings updated");
    },
  });
}
