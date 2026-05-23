import { httpClient } from "@/shared/api";
import type {
  FillBlankRequest,
  FillBlankResult,
  MyVocabularyResponse,
  PronunciationAttemptRequest,
  PronunciationAttemptResponse,
  ReviewRequest,
  ReviewResult,
  StudyCard,
  StudySettings,
  TodayVocabularyResponse,
  UpdateStudySettingsRequest,
} from "../types/vocabulary";

export const vocabularyApi = {
  getToday: (targetLanguageCode: string) =>
    httpClient.get<TodayVocabularyResponse>("/api/vocabulary/today", {
      params: { targetLanguageCode },
    }),

  getMyWords: (targetLanguageCode: string) =>
    httpClient.get<MyVocabularyResponse>("/api/vocabulary/my-words", {
      params: { targetLanguageCode },
    }),

  getStudyCard: (id: string, targetLanguageCode: string, nativeLanguageCode: string) =>
    httpClient.get<StudyCard>(`/api/vocabulary/${id}/study-card`, {
      params: { targetLanguageCode, nativeLanguageCode },
    }),

  review: (id: string, targetLanguageCode: string, data: ReviewRequest) =>
    httpClient.post<ReviewResult>(`/api/vocabulary/${id}/review`, data, {
      params: { targetLanguageCode },
    }),

  submitPronunciation: (
    id: string,
    targetLanguageCode: string,
    data: PronunciationAttemptRequest,
  ) =>
    httpClient.post<PronunciationAttemptResponse>(
      `/api/vocabulary/${id}/pronunciation-attempts`,
      data,
      { params: { targetLanguageCode } },
    ),

  markMastered: (id: string, targetLanguageCode: string) =>
    httpClient.post<ReviewResult>(`/api/vocabulary/${id}/mark-mastered`, undefined, {
      params: { targetLanguageCode },
    }),

  submitFillBlank: (exampleId: string, targetLanguageCode: string, data: FillBlankRequest) =>
    httpClient.post<FillBlankResult>(
      `/api/vocabulary/examples/${exampleId}/fill-blank`,
      data,
      { params: { targetLanguageCode } },
    ),

  getSettings: (targetLanguageCode: string) =>
    httpClient.get<StudySettings>("/api/vocabulary/settings", {
      params: { targetLanguageCode },
    }),

  updateSettings: (targetLanguageCode: string, data: UpdateStudySettingsRequest) =>
    httpClient.put<StudySettings>("/api/vocabulary/settings", data, {
      params: { targetLanguageCode },
    }),
};
