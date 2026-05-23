import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { studyPlansApi } from "../api/study-plans-api";
import { studyPlanKeys } from "../api/query-keys";

export function useSkipSession() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => studyPlansApi.skipSession(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: studyPlanKeys.all });
      toast.success("Session skipped");
    },
  });
}
