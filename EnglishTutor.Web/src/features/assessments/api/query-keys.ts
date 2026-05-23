export const assessmentKeys = {
  all: ["assessments"] as const,
  available: (targetLanguageCode?: string) =>
    [...assessmentKeys.all, "available", targetLanguageCode] as const,
  attempt: (attemptId: string) =>
    [...assessmentKeys.all, "attempt", attemptId] as const,
  result: (attemptId: string) =>
    [...assessmentKeys.all, "result", attemptId] as const,
};
