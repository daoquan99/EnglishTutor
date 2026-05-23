export const vocabularyKeys = {
  all: ["vocabulary"] as const,
  today: (lang: string) => [...vocabularyKeys.all, "today", lang] as const,
  myWords: (lang: string) => [...vocabularyKeys.all, "my-words", lang] as const,
  studyCard: (id: string, lang: string) =>
    [...vocabularyKeys.all, "study-card", id, lang] as const,
  settings: (lang: string) => [...vocabularyKeys.all, "settings", lang] as const,
};
