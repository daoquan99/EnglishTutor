import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { adminApi } from "../api/admin-api";
import { adminKeys } from "../api/query-keys";
import type {
  RegisterProviderRequest,
  UpsertModelRequest,
  ConfigureRouteRequest,
} from "../types/admin-reports";

export function useAiProviders() {
  return useQuery({
    queryKey: adminKeys.aiProviders(),
    queryFn: () => adminApi.getAiProviders(),
  });
}

export function useRegisterAiProvider() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: RegisterProviderRequest) =>
      adminApi.registerAiProvider(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.aiProviders() });
      toast.success("AI provider registered");
    },
  });
}

export function useUpsertAiModel() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      providerName,
      data,
    }: {
      providerName: string;
      data: UpsertModelRequest;
    }) => adminApi.upsertAiModel(providerName, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.aiProviders() });
      toast.success("AI model updated");
    },
  });
}

export function useAiRoutes() {
  return useQuery({
    queryKey: adminKeys.aiRoutes(),
    queryFn: () => adminApi.getAiRoutes(),
  });
}

export function useConfigureAiRoute() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      taskType,
      data,
    }: {
      taskType: string;
      data: ConfigureRouteRequest;
    }) => adminApi.configureAiRoute(taskType, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.aiRoutes() });
      toast.success("AI route configured");
    },
  });
}
