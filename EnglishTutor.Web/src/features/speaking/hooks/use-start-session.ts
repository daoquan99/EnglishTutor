import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { speakingApi } from "../api/speaking-api";
import { speakingKeys } from "../api/query-keys";
import type { StartSessionRequest } from "../types/speaking";

export function useStartSession() {
  const router = useRouter();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: StartSessionRequest) => speakingApi.startSession(data),
    onSuccess: (session) => {
      queryClient.invalidateQueries({ queryKey: speakingKeys.sessions() });
      router.push(`/speaking/${session.sessionId}`);
    },
  });
}
