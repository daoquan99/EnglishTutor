import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { mistakesApi } from "../api/mistakes-api";
import { mistakeKeys } from "../api/query-keys";

export function useMarkMistakeMastered() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => mistakesApi.markMastered(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: mistakeKeys.all });
      toast.success("Mastered!");
    },
  });
}
