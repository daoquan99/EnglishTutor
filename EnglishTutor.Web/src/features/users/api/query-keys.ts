export const usersKeys = {
  all: ["users"] as const,
  profile: () => [...usersKeys.all, "profile"] as const,
  languageSettings: () => [...usersKeys.all, "language-settings"] as const,
  targetLanguages: () => [...usersKeys.all, "target-languages"] as const,
};
