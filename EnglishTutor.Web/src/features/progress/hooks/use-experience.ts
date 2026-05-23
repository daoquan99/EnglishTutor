import { useQuery } from "@tanstack/react-query";
import { progressApi } from "../api/progress-api";
import { progressKeys } from "../api/query-keys";

export function useExperience(targetLanguageCode: string | null) {
  return useQuery({
    queryKey: progressKeys.experience(targetLanguageCode ?? ""),
    queryFn: () => progressApi.getExperience(targetLanguageCode!),
    enabled: !!targetLanguageCode,
  });
}
