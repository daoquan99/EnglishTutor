"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { clearAccessToken } from "@/shared/api";
import { authApi } from "../api/auth-api";
import { useAuthStore } from "./use-auth-store";

export function useLogout() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const setUser = useAuthStore((s) => s.setUser);

  return useMutation({
    mutationFn: authApi.logout,
    onSettled: () => {
      setUser(null);
      clearAccessToken();
      queryClient.clear();
      router.push("/login");
    },
  });
}
