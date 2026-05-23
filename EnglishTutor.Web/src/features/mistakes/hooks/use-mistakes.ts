import { useQuery } from "@tanstack/react-query";
import { mistakesApi } from "../api/mistakes-api";
import { mistakeKeys } from "../api/query-keys";

export function useMistakes() {
  return useQuery({
    queryKey: mistakeKeys.list(),
    queryFn: () => mistakesApi.list(),
  });
}
