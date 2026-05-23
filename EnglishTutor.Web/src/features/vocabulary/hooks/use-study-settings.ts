import { useQuery } from "@tanstack/react-query";
import { vocabularyApi } from "../api/vocabulary-api";
import { vocabularyKeys } from "../api/query-keys";

export function useStudySettings(targetLanguageCode: string | null) {
  return useQuery({
    queryKey: vocabularyKeys.settings(targetLanguageCode ?? ""),
    queryFn: () => vocabularyApi.getSettings(targetLanguageCode!),
    enabled: !!targetLanguageCode,
  });
}
