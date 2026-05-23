export interface UserProfile {
  userId: string;
  displayName: string;
  avatarUrl: string | null;
  bio: string | null;
}

export interface UpdateProfileRequest {
  displayName: string;
  avatarUrl?: string | null;
  bio?: string | null;
}

export interface LanguageSettings {
  userId: string;
  nativeLanguageCode: string;
  uiLanguageCode: string;
  explanationLanguageCode: string;
  activeTargetLanguageCode: string;
}

export interface UpdateLanguageSettingsRequest {
  nativeLanguageCode: string;
  uiLanguageCode: string;
  explanationLanguageCode: string;
  activeTargetLanguageCode: string;
}

export interface TargetLanguage {
  id: string;
  userId: string;
  targetLanguageCode: string;
  currentLevel: string;
  targetLevel: string;
  isActive: boolean;
}

export interface AddTargetLanguageRequest {
  targetLanguageCode: string;
  currentLevel: string;
  targetLevel: string;
}

export const LANGUAGE_LEVELS = ["A1", "A2", "B1", "B2", "C1", "C2"] as const;
export type LanguageLevel = (typeof LANGUAGE_LEVELS)[number];

export const COMMON_LANGUAGES = [
  { code: "en", name: "English" },
  { code: "vi", name: "Vietnamese" },
  { code: "ja", name: "Japanese" },
  { code: "ko", name: "Korean" },
  { code: "zh", name: "Chinese" },
  { code: "fr", name: "French" },
  { code: "de", name: "German" },
  { code: "es", name: "Spanish" },
  { code: "pt", name: "Portuguese" },
  { code: "it", name: "Italian" },
  { code: "th", name: "Thai" },
  { code: "ru", name: "Russian" },
] as const;
