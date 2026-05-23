import { useQuery } from "@tanstack/react-query";
import { exercisesApi } from "../api/exercises-api";
import { exerciseKeys } from "../api/query-keys";

export function useExerciseResult(attemptId: string, enabled = true) {
  return useQuery({
    queryKey: exerciseKeys.result(attemptId),
    queryFn: () => exercisesApi.getResult(attemptId),
    enabled,
  });
}
