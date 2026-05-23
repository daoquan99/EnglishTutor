import { useQuery } from "@tanstack/react-query";
import { progressApi } from "../api/progress-api";
import { progressKeys } from "../api/query-keys";

export function useWeeklyProgress(
  targetLanguageCode: string | null,
  year?: number,
  weekNumber?: number,
) {
  return useQuery({
    queryKey: progressKeys.weekly(targetLanguageCode ?? "", year, weekNumber),
    queryFn: () => progressApi.getWeekly(targetLanguageCode!, year, weekNumber),
    enabled: !!targetLanguageCode,
  });
}
