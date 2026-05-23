export interface SpeakingSession {
  sessionId: string;
  sessionType: string;
  topic: string | null;
  targetLanguageCode: string;
  userLevel: string;
  status: string;
  startedAtUtc: string;
  completedAtUtc: string | null;
  conversationScenarioId: string | null;
  currentLineOrder: number | null;
}

export interface SpeakingTurn {
  turnId: string;
  originalText: string;
  correctedText: string;
  naturalVersion: string;
  grammarScore: number;
  vocabularyScore: number;
  overallScore: number;
  feedback: string;
}

export interface SessionSummary {
  sessionId: string;
  averageGrammarScore: number;
  averageVocabularyScore: number;
  averagePronunciationScore: number;
  averageFluencyScore: number;
  overallScore: number;
  totalTurns: number;
  totalMistakes: number;
  strongPoints: string;
  weakPoints: string;
  recommendation: string;
}

export interface StartSessionRequest {
  sessionType: string;
  topic?: string | null;
  conversationScenarioId?: string | null;
}

export const SESSION_TYPES = ["FreeTalk", "TopicDiscussion", "ConversationPractice"] as const;

export const SESSION_STATUS_LABELS: Record<string, string> = {
  Active: "Active",
  Completed: "Completed",
  Abandoned: "Abandoned",
};
