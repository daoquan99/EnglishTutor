import { useMutation } from "@tanstack/react-query";
import { speakingApi } from "../api/speaking-api";

export function useAddTurn(sessionId: string) {
  return useMutation({
    mutationFn: (userText: string) => speakingApi.addTextTurn(sessionId, userText),
  });
}
