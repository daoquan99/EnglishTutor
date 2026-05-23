import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { usersApi } from "../api/users-api";
import { usersKeys } from "../api/query-keys";

export function useActivateTargetLanguage() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => usersApi.activateTargetLanguage(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: usersKeys.targetLanguages() });
      queryClient.invalidateQueries({ queryKey: usersKeys.languageSettings() });
      toast.success("Target language activated");
    },
  });
}
