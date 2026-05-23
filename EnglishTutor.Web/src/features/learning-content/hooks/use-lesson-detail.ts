import { useQuery } from "@tanstack/react-query";
import { learningContentApi } from "../api/learning-content-api";
import { contentKeys } from "../api/query-keys";

export function useLessonDetail(id: string) {
  return useQuery({
    queryKey: contentKeys.lesson(id),
    queryFn: () => learningContentApi.getLesson(id),
  });
}
