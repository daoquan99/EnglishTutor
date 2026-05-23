import { useQuery } from "@tanstack/react-query";
import { studyPlansApi } from "../api/study-plans-api";
import { studyPlanKeys } from "../api/query-keys";

export function usePlannedSessions(params?: {
  fromDateUtc?: string;
  toDateUtc?: string;
  status?: string;
}) {
  return useQuery({
    queryKey: studyPlanKeys.sessions(params),
    queryFn: () => studyPlansApi.getSessions(params),
  });
}
