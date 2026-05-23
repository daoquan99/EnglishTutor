import { useMutation, useQueryClient } from "@tanstack/react-query";
import { assessmentsApi } from "../api/assessments-api";
import { assessmentKeys } from "../api/query-keys";
import type { SubmitAssessmentAnswersRequest } from "../types/assessments";

export function useSubmitAssessmentAnswers(attemptId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: SubmitAssessmentAnswersRequest) =>
      assessmentsApi.submitAnswers(attemptId, data),
    onSuccess: (attempt) => {
      queryClient.setQueryData(assessmentKeys.attempt(attemptId), attempt);
    },
  });
}
