import { useQuery } from "@tanstack/react-query";
import { usersApi } from "../api/users-api";
import { usersKeys } from "../api/query-keys";

export function useTargetLanguages() {
  return useQuery({
    queryKey: usersKeys.targetLanguages(),
    queryFn: usersApi.getTargetLanguages,
  });
}
