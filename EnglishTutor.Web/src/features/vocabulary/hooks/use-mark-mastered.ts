import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { vocabularyApi } from "../api/vocabulary-api";
import { vocabularyKeys } from "../api/query-keys";

export function useMarkMastered(targetLanguageCode: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => vocabularyApi.markMastered(id, targetLanguageCode!),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: vocabularyKeys.today(targetLanguageCode ?? ""),
      });
      toast.success("Marked as mastered!");
    },
  });
}
