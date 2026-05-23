import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { studyPlansApi } from "../api/study-plans-api";
import { studyPlanKeys } from "../api/query-keys";
import type { CreateStudyPlanRequest } from "../types/study-plans";

export function useCreateStudyPlan() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateStudyPlanRequest) => studyPlansApi.create(data),
    onSuccess: (plan) => {
      queryClient.setQueryData(
        studyPlanKeys.plan(plan.targetLanguageCode),
        plan,
      );
      toast.success("Study plan created");
    },
  });
}
