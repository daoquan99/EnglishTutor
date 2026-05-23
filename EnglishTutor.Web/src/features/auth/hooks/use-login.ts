"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { setAccessToken } from "@/shared/api";
import { authApi } from "../api/auth-api";
import { authKeys } from "../api/query-keys";
import type { LoginRequest } from "../types/auth";

export function useLogin() {
  const router = useRouter();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: LoginRequest) => authApi.login(data),
    onSuccess: (response) => {
      setAccessToken(response.accessToken, response.expiresAtUtc);
      queryClient.invalidateQueries({ queryKey: authKeys.me() });
      router.push("/dashboard");
    },
  });
}
