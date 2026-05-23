import { useQuery } from "@tanstack/react-query";
import { progressApi } from "../api/progress-api";
import { progressKeys } from "../api/query-keys";

export function useActivities(targetLanguageCode: string | null) {
  return useQuery({
    queryKey: progressKeys.activities(targetLanguageCode ?? ""),
    queryFn: () => progressApi.getActivities(targetLanguageCode!),
    enabled: !!targetLanguageCode,
  });
}
