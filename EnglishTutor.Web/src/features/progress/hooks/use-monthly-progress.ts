import { useQuery } from "@tanstack/react-query";
import { progressApi } from "../api/progress-api";
import { progressKeys } from "../api/query-keys";

export function useMonthlyProgress(
  targetLanguageCode: string | null,
  year?: number,
  month?: number,
) {
  return useQuery({
    queryKey: progressKeys.monthly(targetLanguageCode ?? "", year, month),
    queryFn: () => progressApi.getMonthly(targetLanguageCode!, year, month),
    enabled: !!targetLanguageCode,
  });
}
