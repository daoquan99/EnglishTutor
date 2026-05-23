import { useQuery } from "@tanstack/react-query";
import { usersApi } from "../api/users-api";
import { usersKeys } from "../api/query-keys";

export function useProfile() {
  return useQuery({
    queryKey: usersKeys.profile(),
    queryFn: usersApi.getProfile,
  });
}
