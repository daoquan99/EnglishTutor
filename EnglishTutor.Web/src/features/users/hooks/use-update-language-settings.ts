import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { usersApi } from "../api/users-api";
import { usersKeys } from "../api/query-keys";
import type { UpdateLanguageSettingsRequest } from "../types/users";

export function useUpdateLanguageSettings() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: UpdateLanguageSettingsRequest) =>
      usersApi.updateLanguageSettings(data),
    onSuccess: (data) => {
      queryClient.setQueryData(usersKeys.languageSettings(), data);
      toast.success("Language settings updated");
    },
  });
}
