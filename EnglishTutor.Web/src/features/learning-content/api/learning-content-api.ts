import { httpClient } from "@/shared/api";
import type {
  LessonListItem,
  LessonDetail,
  LessonListParams,
  LearningPathCard,
  ConversationListItem,
  ConversationDetail,
  ConversationListParams,
} from "../types/learning-content";

export const learningContentApi = {
  listLessons: (params: LessonListParams = {}) =>
    httpClient.get<LessonListItem[]>("/api/lessons", {
      params: params as Record<string, string | number | boolean | undefined>,
    }),

  getLesson: (id: string) =>
    httpClient.get<LessonDetail>(`/api/lessons/${id}`),

  completeLesson: (id: string, durationSeconds = 0) =>
    httpClient.post<LearningPathCard>(`/api/lessons/${id}/complete`, undefined, {
      params: { durationSeconds },
    }),

  getLearningPath: (targetLanguageCode?: string) =>
    httpClient.get<LearningPathCard[]>("/api/learning-path", {
      params: targetLanguageCode ? { targetLanguageCode } : undefined,
    }),

  listConversations: (params: ConversationListParams = {}) =>
    httpClient.get<ConversationListItem[]>("/api/conversations", {
      params: params as Record<string, string | number | boolean | undefined>,
    }),

  getConversation: (id: string) =>
    httpClient.get<ConversationDetail>(`/api/conversations/${id}`),
};
