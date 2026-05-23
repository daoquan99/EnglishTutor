import { useQuery } from "@tanstack/react-query";
import { assessmentsApi } from "../api/assessments-api";
import { assessmentKeys } from "../api/query-keys";

export function useAssessmentResult(attemptId: string, enabled = true) {
  return useQuery({
    queryKey: assessmentKeys.result(attemptId),
    queryFn: () => assessmentsApi.getResult(attemptId),
    enabled,
  });
}
