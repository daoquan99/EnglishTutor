import { useLanguageSettings } from "@/features/users/hooks/use-language-settings";

export function useActiveLanguage() {
  const { data } = useLanguageSettings();
  return data?.activeTargetLanguageCode ?? null;
}
