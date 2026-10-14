export interface AiModelCatalogItem {
  id: string;
  displayName: string;
  modelCode: string;
  supportsTextGeneration: boolean;
  supportsStructuredOutput: boolean;
  supportsAudioInput: boolean;
  supportsPronunciationAssessment: boolean;
  supportsLiveAudio: boolean;
  supportsTranscription: boolean;
  supportsTranslation: boolean;
  supportsTts: boolean;
  supportsStreaming: boolean;
  isAvailable: boolean;
  isDeprecated: boolean;
  isEnabled: boolean;
  replacementModelCode?: string | null;
  capabilityNote?: string | null;
  displayOrder: number;
}

export interface PronunciationVariantDto {
  id: string;
  accent: 'UK' | 'US' | string;
  ipa: string;
  audioUrl?: string | null;
  audioSource: string;
  syllableCount?: number | null;
  stressPattern?: string | null;
}

export interface WordFamilyItemDto {
  id: string;
  relatedWord: string;
  partOfSpeech: string;
  relatedVocabularyId?: string | null;
}

export interface CollocationItemDto {
  id: string;
  phraseEn: string;
  meaningVi?: string | null;
  displayOrder: number;
}

export interface ExampleSentenceDto {
  id: string;
  sentenceEn: string;
  sentenceVi?: string | null;
  audioUrl?: string | null;
  cefrLevel?: string | null;
  displayOrder: number;
}

export interface GrammarPatternDto {
  id: string;
  pattern: string;
  explanation?: string | null;
  example?: string | null;
}

export interface VocabularyFlashcardDto {
  dailyItemId: string;
  vocabularyId: string;
  senseId: string;
  word: string;
  partOfSpeech: string;
  cefrLevel: string;
  definitionEn: string;
  meaningVi: string;
  descriptionEn?: string | null;
  pronunciations: PronunciationVariantDto[];
  synonyms: string[];
  antonyms: string[];
  wordFamilies: WordFamilyItemDto[];
  collocations: CollocationItemDto[];
  examples: ExampleSentenceDto[];
  grammarPatterns: GrammarPatternDto[];
  itemType: 'New' | 'Review' | 'CarryOver' | string;
  itemStatus: 'Pending' | 'InProgress' | 'Completed' | string;
  learningStatus: 'New' | 'Learning' | 'Review' | 'Mastered' | string;
  masteryLevel: number;
  isNew: boolean;
  isDueForReview: boolean;
  latestPronunciationScore?: number | null;
  nextReviewAt?: string | null;
  intervalDays: number;
  easeFactor: number;
}

export interface DailyVocabularyPlanResponseDto {
  planId: string;
  userId: string;
  learningDate: string;
  timeZoneId: string;
  newVocabularyTarget: number;
  totalAssignedCount: number;
  newCount: number;
  pendingReviewCount: number;
  carryOverCount: number;
  completedCount: number;
  status: string;
  flashcards: VocabularyFlashcardDto[];
}

export interface CriterionScoreResultDto {
  code: string;
  name: string;
  description: string;
  weight: number;
  score: number;
  weightedScore: number;
  feedback: string;
}

export interface PronunciationAssessmentResultDto {
  assessmentId: string;
  vocabularyId: string;
  senseId: string;
  targetType: 'Word' | 'Sentence' | string;
  targetId?: string | null;
  expectedText: string;
  recognizedText?: string | null;
  accent: 'US' | 'UK' | string;
  modelCode: string;
  rubricCode: string;
  rubricVersion: number;
  overallScore?: number | null;
  confidence?: number | null;
  isAssessable: boolean;
  status: string;
  criteria: CriterionScoreResultDto[];
  strengths: string[];
  improvements: string[];
  suggestedPractice: string[];
  createdAt: string;
}
