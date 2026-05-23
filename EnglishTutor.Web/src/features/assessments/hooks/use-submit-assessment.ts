import { useMutation, useQueryClient } from "@tanstack/react-query";
import { assessmentsApi } from "../api/assessments-api";
import { assessmentKeys } from "../api/query-keys";

export function useSubmitAssessment(attemptId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => assessmentsApi.submitAttempt(attemptId),
    onSuccess: (result) => {
      queryClient.setQueryData(assessmentKeys.result(attemptId), result);
    },
  });
}
