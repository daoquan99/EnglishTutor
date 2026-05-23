import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { learningContentApi } from "../api/learning-content-api";
import { contentKeys } from "../api/query-keys";

export function useCompleteLesson() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, durationSeconds }: { id: string; durationSeconds?: number }) =>
      learningContentApi.completeLesson(id, durationSeconds),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: contentKeys.all });
      toast.success("Lesson completed!");
    },
  });
}
