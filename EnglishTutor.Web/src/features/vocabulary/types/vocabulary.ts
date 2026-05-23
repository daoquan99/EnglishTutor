export interface TodayVocabularyItem {
  vocabularyItemId: string;
  word: string;
  masteryStatus: string;
  nextReviewAtUtc: string;
}

export interface TodayVocabularyResponse {
  items: TodayVocabularyItem[];
}

export interface StudyCard {
  vocabularyItemId: string;
  word: string;
  phonetic: string | null;
  partOfSpeech: string;
  topic: string;
  meanings: string[];
  examples: string[];
  meaningMasteryScore: number;
  pronunciationMasteryScore: number;
  exampleSentenceScore: number;
}

export interface ReviewRequest {
  isCorrect: boolean;
  score: number;
}

export interface ReviewResult {
  vocabularyItemId: string;
  masteryStatus: string;
  meaningMasteryScore: number;
  nextReviewAtUtc: string;
}

export interface PronunciationAttemptRequest {
  audioUrl?: string | null;
  recognizedText: string;
  pronunciationScore: number;
  accuracyScore: number;
  fluencyScore: number;
  completenessScore: number;
  feedback: string;
}

export interface PronunciationAttemptResponse {
  attemptId: string;
  pronunciationScore: number;
  accuracyScore: number;
  fluencyScore: number;
  completenessScore: number | null;
  attemptedAtUtc: string;
}

export const MASTERY_STATUS_LABELS: Record<string, string> = {
  New: "New",
  Learning: "Learning",
  Reviewing: "Reviewing",
  Weak: "Weak",
  Mastered: "Mastered",
};

export const MASTERY_STATUS_COLORS: Record<string, string> = {
  New: "text-muted-foreground",
  Learning: "text-blue-500",
  Reviewing: "text-amber-500",
  Weak: "text-red-500",
  Mastered: "text-green-500",
};
