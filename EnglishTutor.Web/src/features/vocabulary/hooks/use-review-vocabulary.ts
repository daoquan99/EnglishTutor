import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { vocabularyApi } from "../api/vocabulary-api";
import { vocabularyKeys } from "../api/query-keys";
import type { ReviewRequest } from "../types/vocabulary";

export function useReviewVocabulary(targetLanguageCode: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ReviewRequest }) =>
      vocabularyApi.review(id, targetLanguageCode!, data),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: vocabularyKeys.today(targetLanguageCode ?? ""),
      });
      toast.success("Review submitted");
    },
  });
}
