import { useQuery } from "@tanstack/react-query";
import { learningContentApi } from "../api/learning-content-api";
import { contentKeys } from "../api/query-keys";
import type { LessonListParams } from "../types/learning-content";

export function useLessons(params: LessonListParams = {}) {
  return useQuery({
    queryKey: contentKeys.lessons(params),
    queryFn: () => learningContentApi.listLessons(params),
    enabled: !!params.targetLanguageCode,
  });
}
