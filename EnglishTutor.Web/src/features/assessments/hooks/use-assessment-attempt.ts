import { useQuery } from "@tanstack/react-query";
import { assessmentsApi } from "../api/assessments-api";
import { assessmentKeys } from "../api/query-keys";

export function useAssessmentAttempt(attemptId: string) {
  return useQuery({
    queryKey: assessmentKeys.attempt(attemptId),
    queryFn: () => assessmentsApi.getAttempt(attemptId),
  });
}
