import { useQuery } from "@tanstack/react-query";
import { progressApi } from "../api/progress-api";
import { progressKeys } from "../api/query-keys";

export function useDashboard(targetLanguageCode: string | null) {
  return useQuery({
    queryKey: progressKeys.dashboard(targetLanguageCode ?? ""),
    queryFn: () => progressApi.getDashboard(targetLanguageCode!),
    enabled: !!targetLanguageCode,
  });
}
