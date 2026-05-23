import { useQuery } from "@tanstack/react-query";
import { speakingApi } from "../api/speaking-api";
import { speakingKeys } from "../api/query-keys";

export function useSessions() {
  return useQuery({
    queryKey: speakingKeys.sessions(),
    queryFn: () => speakingApi.listSessions(),
  });
}
