import { useQuery } from "@tanstack/react-query";
import { exercisesApi } from "../api/exercises-api";
import { exerciseKeys } from "../api/query-keys";
import type { ExerciseListParams } from "../types/exercises";

export function useExercises(params: ExerciseListParams = {}) {
  return useQuery({
    queryKey: exerciseKeys.list(params),
    queryFn: () => exercisesApi.list(params),
    enabled: !!params.targetLanguageCode,
  });
}
