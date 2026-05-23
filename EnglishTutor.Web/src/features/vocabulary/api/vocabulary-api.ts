import { httpClient } from "@/shared/api";
import type {
  PronunciationAttemptRequest,
  PronunciationAttemptResponse,
  ReviewRequest,
  ReviewResult,
  StudyCard,
  TodayVocabularyResponse,
} from "../types/vocabulary";

export const vocabularyApi = {
  getToday: (targetLanguageCode: string) =>
    httpClient.get<TodayVocabularyResponse>("/api/vocabulary/today", {
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
};
