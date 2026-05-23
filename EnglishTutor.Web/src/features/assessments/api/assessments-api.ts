import { httpClient } from "@/shared/api";
import type {
  AvailableAssessment,
  AttemptDetail,
  AttemptResult,
  StartLevelUpRequest,
  SubmitAssessmentAnswersRequest,
} from "../types/assessments";

export const assessmentsApi = {
  getAvailable: (targetLanguageCode?: string) =>
    httpClient.get<AvailableAssessment[]>("/api/assessments/available", {
      params: targetLanguageCode ? { targetLanguageCode } : undefined,
    }),

  startLevelUp: (data: StartLevelUpRequest) =>
    httpClient.post<AttemptDetail>("/api/assessments/level-up", data),

  getAttempt: (attemptId: string) =>
    httpClient.get<AttemptDetail>(`/api/assessments/attempts/${attemptId}`),

  submitAnswers: (attemptId: string, data: SubmitAssessmentAnswersRequest) =>
    httpClient.post<AttemptDetail>(
      `/api/assessments/attempts/${attemptId}/answers`,
      data,
    ),

  submitAttempt: (attemptId: string) =>
    httpClient.post<AttemptResult>(
      `/api/assessments/attempts/${attemptId}/submit`,
    ),

  getResult: (attemptId: string) =>
    httpClient.get<AttemptResult>(
      `/api/assessments/attempts/${attemptId}/result`,
    ),
};
