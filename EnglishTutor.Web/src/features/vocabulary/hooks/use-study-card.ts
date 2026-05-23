import { useQuery } from "@tanstack/react-query";
import { vocabularyApi } from "../api/vocabulary-api";
import { vocabularyKeys } from "../api/query-keys";

export function useStudyCard(
  id: string,
  targetLanguageCode: string | null,
  nativeLanguageCode: string | null,
) {
  return useQuery({
    queryKey: [...vocabularyKeys.studyCard(id, targetLanguageCode ?? ""), nativeLanguageCode],
    queryFn: () =>
      vocabularyApi.getStudyCard(id, targetLanguageCode!, nativeLanguageCode!),
    enabled: !!targetLanguageCode && !!nativeLanguageCode,
  });
}
