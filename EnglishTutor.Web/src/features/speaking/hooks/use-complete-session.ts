import { useMutation, useQueryClient } from "@tanstack/react-query";
import { speakingApi } from "../api/speaking-api";
import { speakingKeys } from "../api/query-keys";

export function useCompleteSession(sessionId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => speakingApi.completeSession(sessionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: speakingKeys.sessions() });
      queryClient.invalidateQueries({ queryKey: speakingKeys.session(sessionId) });
    },
  });
}
