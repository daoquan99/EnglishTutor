export interface Mistake {
  id: string;
  targetLanguageCode: string;
  type: string;
  category: string;
  originalText: string;
  correctedText: string;
  explanation: string;
  sourceType: string;
  sourceId: string;
  status: string;
  nextReviewAtUtc: string;
  createdAtUtc: string;
}

export const MISTAKE_STATUS_LABELS: Record<string, string> = {
  New: "New",
  Reviewing: "Reviewing",
  Mastered: "Mastered",
};
