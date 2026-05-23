import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { adminApi } from "../api/admin-api";
import { adminKeys } from "../api/query-keys";

export function useUserOverview(params?: { page?: number; pageSize?: number }) {
  return useQuery({
    queryKey: adminKeys.users(params),
    queryFn: () => adminApi.getUserOverview(params),
  });
}

export function useAiUsageReport(params?: {
  from?: string;
  to?: string;
  modelType?: string;
}) {
  return useQuery({
    queryKey: adminKeys.aiUsage(params),
    queryFn: () => adminApi.getAiUsage(params),
  });
}

export function useLearningActivityReport(params?: {
  from?: string;
  to?: string;
  period?: string;
}) {
  return useQuery({
    queryKey: adminKeys.learningActivity(params),
    queryFn: () => adminApi.getLearningActivity(params),
  });
}

export function useCommonMistakesReport(params?: {
  targetLanguageCode?: string;
  top?: number;
}) {
  return useQuery({
    queryKey: adminKeys.commonMistakes(params),
    queryFn: () => adminApi.getCommonMistakes(params),
  });
}

export function useAssessmentPassRates(params?: {
  from?: string;
  to?: string;
  targetLanguageCode?: string;
}) {
  return useQuery({
    queryKey: adminKeys.assessmentPassRates(params),
    queryFn: () => adminApi.getAssessmentPassRates(params),
  });
}

export function useAuditLogs(params?: {
  page?: number;
  pageSize?: number;
  action?: string;
  targetEntity?: string;
}) {
  return useQuery({
    queryKey: adminKeys.auditLogs(params),
    queryFn: () => adminApi.getAuditLogs(params),
  });
}

export function useDeadLetters(params?: {
  page?: number;
  pageSize?: number;
  sourceModule?: string;
  eventType?: string;
  status?: string;
}) {
  return useQuery({
    queryKey: adminKeys.deadLetters(params),
    queryFn: () => adminApi.getDeadLetters(params),
  });
}

export function useReprocessDeadLetter() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => adminApi.reprocessDeadLetter(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.deadLetters() });
      toast.success("Dead letter reprocessed");
    },
  });
}
