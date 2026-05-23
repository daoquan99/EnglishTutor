import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { studyPlansApi } from "../api/study-plans-api";
import { studyPlanKeys } from "../api/query-keys";
import type { UpdateScheduleRequest } from "../types/study-plans";

export function useUpdateSchedule(targetLanguageCode?: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: UpdateScheduleRequest) =>
      studyPlansApi.updateSchedule(data, targetLanguageCode),
    onSuccess: (days) => {
      queryClient.setQueryData(
        studyPlanKeys.schedule(targetLanguageCode),
        days,
      );
      toast.success("Schedule updated");
    },
  });
}
