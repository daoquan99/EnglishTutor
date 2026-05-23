import { useMutation, useQueryClient } from "@tanstack/react-query";
import { vocabularyApi } from "../api/vocabulary-api";
import { vocabularyKeys } from "../api/query-keys";
import type { FillBlankRequest } from "../types/vocabulary";

export function useFillBlank(targetLanguageCode: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ exampleId, data }: { exampleId: string; data: FillBlankRequest }) =>
      vocabularyApi.submitFillBlank(exampleId, targetLanguageCode!, data),
    onSuccess: (_result, { exampleId }) => {
      queryClient.invalidateQueries({
        queryKey: vocabularyKeys.all,
        predicate: (query) =>
          query.queryKey[1] === "study-card" ||
          (query.queryKey[1] === "today" && query.queryKey[2] === targetLanguageCode),
      });
    },
  });
}
