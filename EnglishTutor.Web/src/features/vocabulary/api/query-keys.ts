export const vocabularyKeys = {
  all: ["vocabulary"] as const,
  today: (lang: string) => [...vocabularyKeys.all, "today", lang] as const,
  studyCard: (id: string, lang: string) =>
    [...vocabularyKeys.all, "study-card", id, lang] as const,
};
