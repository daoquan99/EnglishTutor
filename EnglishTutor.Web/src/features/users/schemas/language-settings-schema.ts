import { z } from "zod";

const langCode = z.string().min(2, "Language code is required").max(3);

export const languageSettingsSchema = z.object({
  nativeLanguageCode: langCode,
  uiLanguageCode: langCode,
  explanationLanguageCode: langCode,
  activeTargetLanguageCode: langCode,
});

export type LanguageSettingsFormValues = z.infer<typeof languageSettingsSchema>;
