import { useQuery } from "@tanstack/react-query";
import { vocabularyApi } from "../api/vocabulary-api";
import { vocabularyKeys } from "../api/query-keys";

export function useMyVocabulary(targetLanguageCode: string | null) {
  return useQuery({
    queryKey: vocabularyKeys.myWords(targetLanguageCode ?? ""),
    queryFn: () => vocabularyApi.getMyWords(targetLanguageCode!),
    enabled: !!targetLanguageCode,
  });
}
