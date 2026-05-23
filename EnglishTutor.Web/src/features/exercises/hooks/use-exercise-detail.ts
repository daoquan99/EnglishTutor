import { useQuery } from "@tanstack/react-query";
import { exercisesApi } from "../api/exercises-api";
import { exerciseKeys } from "../api/query-keys";

export function useExerciseDetail(id: string) {
  return useQuery({
    queryKey: exerciseKeys.detail(id),
    queryFn: () => exercisesApi.getDetail(id),
  });
}
