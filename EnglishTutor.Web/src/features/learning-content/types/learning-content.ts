export interface LessonListItem {
  id: string;
  targetLanguageCode: string;
  level: string;
  topic: string;
  skill: string;
  title: string;
  description: string;
  order: number;
  estimatedMinutes: number;
}

export interface LessonDetail {
  id: string;
  targetLanguageCode: string;
  level: string;
  topic: string;
  skill: string;
  title: string;
  description: string;
  order: number;
  estimatedMinutes: number;
  sections: LessonSection[];
}

export interface LessonSection {
  id: string;
  title: string;
  content: string;
  order: number;
  sectionType: string;
}

export interface LearningPathCard {
  id: string;
  targetLanguageCode: string;
  contentType: string;
  contentId: string;
  title: string;
  level: string;
  skill: string | null;
  status: string;
  order: number;
  completedAtUtc: string | null;
  lastUpdatedAtUtc: string;
}

export interface ConversationListItem {
  id: string;
  targetLanguageCode: string;
  level: string;
  title: string;
  description: string;
  setting: string;
  difficulty: number;
  estimatedMinutes: number;
}

export interface ConversationDetail {
  id: string;
  targetLanguageCode: string;
  level: string;
  title: string;
  description: string;
  setting: string;
  difficulty: number;
  estimatedMinutes: number;
  lines: ConversationLine[];
}

export interface ConversationLine {
  id: string;
  order: number;
  speaker: string;
  text: string;
  expectedResponseHint: string | null;
  audioUrl: string | null;
  notes: string | null;
}

export interface LessonListParams {
  page?: number;
  pageSize?: number;
  level?: string;
  topic?: string;
  skill?: string;
  targetLanguageCode?: string;
}

export interface ConversationListParams {
  page?: number;
  pageSize?: number;
  level?: string;
  difficulty?: number;
  targetLanguageCode?: string;
}
