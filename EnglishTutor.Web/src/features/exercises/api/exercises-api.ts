import { httpClient } from "@/shared/api";
import type {
  ExerciseListItem,
  ExerciseDetail,
  ExerciseListParams,
  StartAttemptResponse,
  SubmitAnswerRequest,
  SubmitAnswerResponse,
  ExerciseResult,
} from "../types/exercises";

export const exercisesApi = {
  list: (params: ExerciseListParams = {}) =>
    httpClient.get<ExerciseListItem[]>("/api/exercises", {
      params: params as Record<string, string | number | boolean | undefined>,
    }),

  getDetail: (id: string) =>
    httpClient.get<ExerciseDetail>(`/api/exercises/${id}`),

  startAttempt: (exerciseId: string) =>
    httpClient.post<StartAttemptResponse>(`/api/exercises/${exerciseId}/attempts`),

  submitAnswer: (attemptId: string, data: SubmitAnswerRequest) =>
    httpClient.post<SubmitAnswerResponse>(
      `/api/exercises/attempts/${attemptId}/answers`,
      data,
    ),

  completeAttempt: (attemptId: string) =>
    httpClient.post<ExerciseResult>(
      `/api/exercises/attempts/${attemptId}/complete`,
    ),

  getResult: (attemptId: string) =>
    httpClient.get<ExerciseResult>(
      `/api/exercises/attempts/${attemptId}/result`,
    ),
};
