import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { vocabularyApi } from "../api/vocabulary-api";
import { vocabularyKeys } from "../api/query-keys";
import type { UpdateStudySettingsRequest } from "../types/vocabulary";

export function useUpdateStudySettings(targetLanguageCode: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: UpdateStudySettingsRequest) =>
      vocabularyApi.updateSettings(targetLanguageCode!, data),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: vocabularyKeys.settings(targetLanguageCode ?? ""),
      });
      queryClient.invalidateQueries({
        queryKey: vocabularyKeys.today(targetLanguageCode ?? ""),
      });
      toast.success("Study settings updated");
    },
  });
}
