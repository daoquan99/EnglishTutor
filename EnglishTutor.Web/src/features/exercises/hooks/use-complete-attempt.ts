import { useMutation, useQueryClient } from "@tanstack/react-query";
import { exercisesApi } from "../api/exercises-api";
import { exerciseKeys } from "../api/query-keys";

export function useCompleteAttempt(attemptId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => exercisesApi.completeAttempt(attemptId),
    onSuccess: (result) => {
      queryClient.setQueryData(exerciseKeys.result(attemptId), result);
    },
  });
}
