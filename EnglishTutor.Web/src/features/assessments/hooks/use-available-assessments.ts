import { useQuery } from "@tanstack/react-query";
import { assessmentsApi } from "../api/assessments-api";
import { assessmentKeys } from "../api/query-keys";

export function useAvailableAssessments(targetLanguageCode?: string) {
  return useQuery({
    queryKey: assessmentKeys.available(targetLanguageCode),
    queryFn: () => assessmentsApi.getAvailable(targetLanguageCode),
    enabled: !!targetLanguageCode,
  });
}
