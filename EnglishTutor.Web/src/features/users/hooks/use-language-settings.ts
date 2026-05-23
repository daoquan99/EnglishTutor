import { useQuery } from "@tanstack/react-query";
import { usersApi } from "../api/users-api";
import { usersKeys } from "../api/query-keys";

export function useLanguageSettings() {
  return useQuery({
    queryKey: usersKeys.languageSettings(),
    queryFn: usersApi.getLanguageSettings,
  });
}
