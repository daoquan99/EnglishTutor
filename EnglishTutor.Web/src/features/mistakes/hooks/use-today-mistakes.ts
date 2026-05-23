import { useQuery } from "@tanstack/react-query";
import { mistakesApi } from "../api/mistakes-api";
import { mistakeKeys } from "../api/query-keys";

export function useTodayMistakes() {
  return useQuery({
    queryKey: mistakeKeys.today(),
    queryFn: () => mistakesApi.getToday(),
  });
}
