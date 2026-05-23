import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { studyPlansApi } from "../api/study-plans-api";
import { studyPlanKeys } from "../api/query-keys";
import type { UpdateStudyPlanRequest } from "../types/study-plans";

export function useUpdateStudyPlan(targetLanguageCode?: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: UpdateStudyPlanRequest) =>
      studyPlansApi.update(data, targetLanguageCode),
    onSuccess: (plan) => {
      queryClient.setQueryData(studyPlanKeys.plan(targetLanguageCode), plan);
      toast.success("Study plan updated");
    },
  });
}
