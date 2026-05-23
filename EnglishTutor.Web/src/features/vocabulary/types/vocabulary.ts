export interface TodayVocabularyItem {
  vocabularyItemId: string;
  word: string;
  phonetic: string | null;
  masteryStatus: string;
  meaningMasteryScore: number;
  reviewCount: number;
  nextReviewAtUtc: string;
}

export interface TodayVocabularyResponse {
  newItems: TodayVocabularyItem[];
  reviewItems: TodayVocabularyItem[];
  newWordsCount: number;
  reviewWordsCount: number;
  newWordsPerDay: number;
}

export interface StudyCardExample {
  exampleId: string;
  sentence: string;
  fillBlankSentence: string | null;
  targetWord: string;
  translations: string[];
}

export interface StudyCard {
  vocabularyItemId: string;
  word: string;
  phonetic: string | null;
  partOfSpeech: string;
  topic: string;
  meanings: string[];
  examples: StudyCardExample[];
  masteryStatus: string;
  meaningMasteryScore: number;
  pronunciationMasteryScore: number;
  exampleSentenceScore: number;
  reviewCount: number;
  consecutiveCorrectCount: number;
  canMarkMastered: boolean;
}

export interface ReviewRequest {
  isCorrect: boolean;
  score: number;
}

export interface ReviewResult {
  vocabularyItemId: string;
  masteryStatus: string;
  meaningMasteryScore: number;
  pronunciationMasteryScore: number;
  exampleSentenceScore: number;
  reviewCount: number;
  consecutiveCorrectCount: number;
  canMarkMastered: boolean;
  nextReviewAtUtc: string;
}

export interface FillBlankRequest {
  userAnswer: string;
}

export interface FillBlankResult {
  attemptId: string;
  isCorrect: boolean;
  correctAnswer: string;
  fullSentence: string;
  exampleSentenceScore: number;
}

export interface StudySettings {
  newWordsPerDay: number;
  reviewWordsPerDay: number;
  includeMasteredInReview: boolean;
}

export interface UpdateStudySettingsRequest {
  newWordsPerDay: number;
  reviewWordsPerDay: number;
  includeMasteredInReview: boolean;
}

export interface MyVocabularyItem {
  vocabularyItemId: string;
  word: string;
  phonetic: string | null;
  masteryStatus: string;
  meaningMasteryScore: number;
  reviewCount: number;
  nextReviewAtUtc: string | null;
}

export interface MyVocabularyCounts {
  new: number;
  learning: number;
  reviewing: number;
  weak: number;
  mastered: number;
  total: number;
}

export interface MyVocabularyResponse {
  items: MyVocabularyItem[];
  counts: MyVocabularyCounts;
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
