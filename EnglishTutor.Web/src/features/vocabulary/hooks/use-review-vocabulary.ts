import { useMutation, useQueryClient } from "@tanstack/react-query";
import { vocabularyApi } from "../api/vocabulary-api";
import { vocabularyKeys } from "../api/query-keys";
import type { ReviewRequest, ReviewResult, StudyCard } from "../types/vocabulary";

export function useReviewVocabulary(targetLanguageCode: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ReviewRequest }) =>
      vocabularyApi.review(id, targetLanguageCode!, data),
    onSuccess: (result: ReviewResult, { id }) => {
      queryClient.setQueriesData<StudyCard>(
        { queryKey: vocabularyKeys.studyCard(id, targetLanguageCode ?? "") },
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
    },
  });
}
