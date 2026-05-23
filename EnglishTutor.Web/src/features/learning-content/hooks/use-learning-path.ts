import { useQuery } from "@tanstack/react-query";
import { learningContentApi } from "../api/learning-content-api";
import { contentKeys } from "../api/query-keys";

export function useLearningPath(targetLanguageCode?: string) {
  return useQuery({
    queryKey: contentKeys.learningPath(targetLanguageCode),
    queryFn: () => learningContentApi.getLearningPath(targetLanguageCode),
    enabled: !!targetLanguageCode,
  });
}
