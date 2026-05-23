import { useQuery } from "@tanstack/react-query";
import { speakingApi } from "../api/speaking-api";
import { speakingKeys } from "../api/query-keys";

export function useSessionSummary(sessionId: string, enabled = true) {
  return useQuery({
    queryKey: speakingKeys.summary(sessionId),
    queryFn: () => speakingApi.getSummary(sessionId),
    enabled,
  });
}
