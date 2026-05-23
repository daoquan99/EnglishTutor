import { httpClient } from "@/shared/api";
import type {
  StudyPlan,
  CreateStudyPlanRequest,
  UpdateStudyPlanRequest,
  WeekDay,
  UpdateScheduleRequest,
  PlannedSession,
} from "../types/study-plans";

export const studyPlansApi = {
  get: (targetLanguageCode?: string) =>
    httpClient.get<StudyPlan>("/api/study-plans/me", {
      params: targetLanguageCode ? { targetLanguageCode } : undefined,
    }),

  create: (data: CreateStudyPlanRequest) =>
    httpClient.post<StudyPlan>("/api/study-plans/me", data),

  update: (data: UpdateStudyPlanRequest, targetLanguageCode?: string) =>
    httpClient.put<StudyPlan>("/api/study-plans/me", data, {
      params: targetLanguageCode ? { targetLanguageCode } : undefined,
    }),

  getSchedule: (targetLanguageCode?: string) =>
    httpClient.get<WeekDay[]>("/api/study-plans/me/schedule", {
      params: targetLanguageCode ? { targetLanguageCode } : undefined,
    }),

  updateSchedule: (data: UpdateScheduleRequest, targetLanguageCode?: string) =>
    httpClient.put<WeekDay[]>("/api/study-plans/me/schedule", data, {
      params: targetLanguageCode ? { targetLanguageCode } : undefined,
    }),

  getSessions: (params?: {
    fromDateUtc?: string;
    toDateUtc?: string;
    status?: string;
  }) =>
    httpClient.get<PlannedSession[]>("/api/study-plans/me/planned-sessions", {
      params: params as
        | Record<string, string | number | boolean | undefined>
        | undefined,
    }),

  skipSession: (id: string) =>
    httpClient.post<PlannedSession>(
      `/api/study-plans/me/planned-sessions/${id}/skip`,
    ),
};
