import { httpClient } from "@/shared/api";
import type {
  AddTargetLanguageRequest,
  LanguageSettings,
  TargetLanguage,
  UpdateLanguageSettingsRequest,
  UpdateProfileRequest,
  UserProfile,
} from "../types/users";

export const usersApi = {
  getProfile: () => httpClient.get<UserProfile>("/api/users/me/profile"),

  updateProfile: (data: UpdateProfileRequest) =>
    httpClient.put<UserProfile>("/api/users/me/profile", data),

  getLanguageSettings: () =>
    httpClient.get<LanguageSettings>("/api/users/me/language-settings"),

  updateLanguageSettings: (data: UpdateLanguageSettingsRequest) =>
    httpClient.put<LanguageSettings>("/api/users/me/language-settings", data),

  getTargetLanguages: () =>
    httpClient.get<TargetLanguage[]>("/api/users/me/target-languages"),

  addTargetLanguage: (data: AddTargetLanguageRequest) =>
    httpClient.post<TargetLanguage>("/api/users/me/target-languages", data),

  activateTargetLanguage: (id: string) =>
    httpClient.put<void>(`/api/users/me/target-languages/${id}/activate`),
};
