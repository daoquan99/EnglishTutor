import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { assessmentsApi } from "../api/assessments-api";
import { assessmentKeys } from "../api/query-keys";
import type { StartLevelUpRequest } from "../types/assessments";

export function useStartLevelUp() {
  const router = useRouter();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: StartLevelUpRequest) => assessmentsApi.startLevelUp(data),
    onSuccess: (attempt) => {
      queryClient.setQueryData(assessmentKeys.attempt(attempt.attemptId), attempt);
      router.push(`/assessments/${attempt.attemptId}`);
    },
  });
}
