import type { ConversationListParams, LessonListParams } from "../types/learning-content";

export const contentKeys = {
  all: ["learning-content"] as const,
  lessons: (params: LessonListParams) =>
    [...contentKeys.all, "lessons", params] as const,
  lesson: (id: string) => [...contentKeys.all, "lesson", id] as const,
  learningPath: (targetLanguageCode?: string) =>
    [...contentKeys.all, "learning-path", targetLanguageCode] as const,
  conversations: (params: ConversationListParams) =>
    [...contentKeys.all, "conversations", params] as const,
  conversation: (id: string) =>
    [...contentKeys.all, "conversation", id] as const,
};
