import { useQuery } from "@tanstack/react-query";
import { vocabularyApi } from "../api/vocabulary-api";
import { vocabularyKeys } from "../api/query-keys";

export function useTodayVocabulary(targetLanguageCode: string | null) {
  return useQuery({
    queryKey: vocabularyKeys.today(targetLanguageCode ?? ""),
    queryFn: () => vocabularyApi.getToday(targetLanguageCode!),
    enabled: !!targetLanguageCode,
  });
}
