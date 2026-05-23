"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { setAccessToken } from "@/shared/api";
import { authApi } from "../api/auth-api";
import { authKeys } from "../api/query-keys";
import type { RegisterRequest } from "../types/auth";

export function useRegister() {
  const router = useRouter();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: RegisterRequest) => authApi.register(data),
    meta: { skipGlobalErrorToast: true },
    onSuccess: (response) => {
      setAccessToken(response.accessToken, response.expiresAtUtc);
      queryClient.invalidateQueries({ queryKey: authKeys.me() });
      router.push("/dashboard");
    },
  });
}
