"use client";

import { useQuery } from "@tanstack/react-query";
import { useEffect } from "react";
import { authApi } from "../api/auth-api";
import { authKeys } from "../api/query-keys";
import { useAuthStore } from "./use-auth-store";

export function useCurrentUser() {
  const setUser = useAuthStore((s) => s.setUser);

  const query = useQuery({
    queryKey: authKeys.me(),
    queryFn: authApi.me,
    retry: false,
    staleTime: 5 * 60 * 1000,
  });

  useEffect(() => {
    setUser(query.data ?? null);
  }, [query.data, setUser]);

  return query;
}
