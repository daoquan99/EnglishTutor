import { useMutation } from "@tanstack/react-query";
import { exercisesApi } from "../api/exercises-api";
import type { SubmitAnswerRequest } from "../types/exercises";

export function useSubmitAnswer(attemptId: string) {
  return useMutation({
    mutationFn: (data: SubmitAnswerRequest) =>
      exercisesApi.submitAnswer(attemptId, data),
  });
}
