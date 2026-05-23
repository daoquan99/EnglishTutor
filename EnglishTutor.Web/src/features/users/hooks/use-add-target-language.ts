import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { usersApi } from "../api/users-api";
import { usersKeys } from "../api/query-keys";
import type { AddTargetLanguageRequest } from "../types/users";

export function useAddTargetLanguage() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: AddTargetLanguageRequest) => usersApi.addTargetLanguage(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: usersKeys.targetLanguages() });
      toast.success("Target language added");
    },
  });
}
