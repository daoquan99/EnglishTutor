import { useQuery } from "@tanstack/react-query";
import { speakingApi } from "../api/speaking-api";
import { speakingKeys } from "../api/query-keys";

export function useSession(sessionId: string) {
  return useQuery({
    queryKey: speakingKeys.session(sessionId),
    queryFn: () => speakingApi.getSession(sessionId),
  });
}
