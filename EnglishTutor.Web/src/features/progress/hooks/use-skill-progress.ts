import { useQuery } from "@tanstack/react-query";
import { progressApi } from "../api/progress-api";
import { progressKeys } from "../api/query-keys";

export function useSkillProgress(targetLanguageCode: string | null) {
  return useQuery({
    queryKey: progressKeys.skills(targetLanguageCode ?? ""),
    queryFn: () => progressApi.getSkills(targetLanguageCode!),
    enabled: !!targetLanguageCode,
  });
}
