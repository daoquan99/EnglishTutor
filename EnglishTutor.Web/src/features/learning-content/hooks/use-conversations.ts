import { useQuery } from "@tanstack/react-query";
import { learningContentApi } from "../api/learning-content-api";
import { contentKeys } from "../api/query-keys";
import type { ConversationListParams } from "../types/learning-content";

export function useConversations(params: ConversationListParams = {}) {
  return useQuery({
    queryKey: contentKeys.conversations(params),
    queryFn: () => learningContentApi.listConversations(params),
    enabled: !!params.targetLanguageCode,
  });
}
