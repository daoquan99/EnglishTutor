import { useQuery } from "@tanstack/react-query";
import { learningContentApi } from "../api/learning-content-api";
import { contentKeys } from "../api/query-keys";

export function useConversationDetail(id: string) {
  return useQuery({
    queryKey: contentKeys.conversation(id),
    queryFn: () => learningContentApi.getConversation(id),
  });
}
