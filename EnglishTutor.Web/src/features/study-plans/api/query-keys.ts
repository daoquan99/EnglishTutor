export const studyPlanKeys = {
  all: ["study-plans"] as const,
  plan: (targetLanguageCode?: string) =>
    [...studyPlanKeys.all, "plan", targetLanguageCode] as const,
  schedule: (targetLanguageCode?: string) =>
    [...studyPlanKeys.all, "schedule", targetLanguageCode] as const,
  sessions: (params?: Record<string, unknown>) =>
    [...studyPlanKeys.all, "sessions", params] as const,
};
