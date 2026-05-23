import { useQuery } from "@tanstack/react-query";
import { studyPlansApi } from "../api/study-plans-api";
import { studyPlanKeys } from "../api/query-keys";

export function useStudyPlan(targetLanguageCode?: string) {
  return useQuery({
    queryKey: studyPlanKeys.plan(targetLanguageCode),
    queryFn: () => studyPlansApi.get(targetLanguageCode),
    enabled: !!targetLanguageCode,
  });
}
