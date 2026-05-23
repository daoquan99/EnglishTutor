import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { vocabularyApi } from "../api/vocabulary-api";
import { vocabularyKeys } from "../api/query-keys";
import type { ReviewResult, StudyCard } from "../types/vocabulary";

export function useMarkMastered(targetLanguageCode: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => vocabularyApi.markMastered(id, targetLanguageCode!),
    onSuccess: (result: ReviewResult) => {
      queryClient.setQueriesData<StudyCard>(
        { queryKey: vocabularyKeys.studyCard(result.vocabularyItemId, targetLanguageCode ?? "") },
        (old) =>
          old
            ? {
                ...old,
                masteryStatus: result.masteryStatus,
                meaningMasteryScore: result.meaningMasteryScore,
                pronunciationMasteryScore: result.pronunciationMasteryScore,
                exampleSentenceScore: result.exampleSentenceScore,
                reviewCount: result.reviewCount,
                consecutiveCorrectCount: result.consecutiveCorrectCount,
                canMarkMastered: result.canMarkMastered,
              }
            : old,
      );
      queryClient.invalidateQueries({
        queryKey: vocabularyKeys.today(targetLanguageCode ?? ""),
      });
      toast.success("Vocabulary mastered!");
    },
  });
}
