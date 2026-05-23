import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { usersApi } from "../api/users-api";
import { usersKeys } from "../api/query-keys";
import type { UpdateProfileRequest } from "../types/users";

export function useUpdateProfile() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: UpdateProfileRequest) => usersApi.updateProfile(data),
    onSuccess: (data) => {
      queryClient.setQueryData(usersKeys.profile(), data);
      toast.success("Profile updated");
    },
  });
}
