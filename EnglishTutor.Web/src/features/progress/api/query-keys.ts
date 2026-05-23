export const progressKeys = {
  all: ["progress"] as const,
  dashboard: (lang: string) => [...progressKeys.all, "dashboard", lang] as const,
  experience: (lang: string) => [...progressKeys.all, "experience", lang] as const,
  activities: (lang: string) => [...progressKeys.all, "activities", lang] as const,
  weekly: (lang: string, year?: number, week?: number) =>
    [...progressKeys.all, "weekly", lang, year, week] as const,
  monthly: (lang: string, year?: number, month?: number) =>
    [...progressKeys.all, "monthly", lang, year, month] as const,
  skills: (lang: string) => [...progressKeys.all, "skills", lang] as const,
};
