import { httpClient } from "@/shared/api";
import type {
  UserOverviewCard,
  DailyAiUsageReport,
  LearningActivityReport,
  CommonMistakeStat,
  AssessmentPassRateReport,
  AuditLog,
  DeadLetterMessage,
  AuthPermission,
  AuthRole,
  CreatePermissionRequest,
  UpdatePermissionRequest,
  CreateRoleRequest,
  UpdateRoleRequest,
  AiProvider,
  AiRuntimeRoute,
  RegisterProviderRequest,
  UpsertModelRequest,
  ConfigureRouteRequest,
} from "../types/admin-reports";

type P = Record<string, string | number | boolean | undefined>;

export const adminApi = {
  // Reports
  getUserOverview: (params?: { page?: number; pageSize?: number }) =>
    httpClient.get<UserOverviewCard[]>("/api/admin/reports/users", {
      params: params as P | undefined,
    }),

  getAiUsage: (params?: { from?: string; to?: string; modelType?: string }) =>
    httpClient.get<DailyAiUsageReport[]>("/api/admin/reports/ai-usage", {
      params: params as P | undefined,
    }),

  getLearningActivity: (params?: { from?: string; to?: string; period?: string }) =>
    httpClient.get<LearningActivityReport[]>("/api/admin/reports/learning-activity", {
      params: params as P | undefined,
    }),

  getCommonMistakes: (params?: { targetLanguageCode?: string; top?: number }) =>
    httpClient.get<CommonMistakeStat[]>("/api/admin/reports/mistakes", {
      params: params as P | undefined,
    }),

  getAssessmentPassRates: (params?: {
    from?: string;
    to?: string;
    targetLanguageCode?: string;
  }) =>
    httpClient.get<AssessmentPassRateReport[]>("/api/admin/reports/assessments", {
      params: params as P | undefined,
    }),

  getAuditLogs: (params?: {
    page?: number;
    pageSize?: number;
    action?: string;
    targetEntity?: string;
  }) =>
    httpClient.get<AuditLog[]>("/api/admin/audit-logs", {
      params: params as P | undefined,
    }),

  getDeadLetters: (params?: {
    page?: number;
    pageSize?: number;
    sourceModule?: string;
    eventType?: string;
    status?: string;
  }) =>
    httpClient.get<DeadLetterMessage[]>("/api/admin/dead-letters", {
      params: params as P | undefined,
    }),

  reprocessDeadLetter: (id: string) =>
    httpClient.post(`/api/admin/dead-letters/${id}/reprocess`),

  // Auth admin
  getPermissions: (includeDisabled = false) =>
    httpClient.get<AuthPermission[]>("/api/admin/auth/permissions", {
      params: { includeDisabled },
    }),

  createPermission: (data: CreatePermissionRequest) =>
    httpClient.post<AuthPermission>("/api/admin/auth/permissions", data),

  updatePermission: (id: string, data: UpdatePermissionRequest) =>
    httpClient.put<AuthPermission>(`/api/admin/auth/permissions/${id}`, data),

  deletePermission: (id: string) =>
    httpClient.delete(`/api/admin/auth/permissions/${id}`),

  getRoles: (includeDisabled = false) =>
    httpClient.get<AuthRole[]>("/api/admin/auth/roles", {
      params: { includeDisabled },
    }),

  getRole: (id: string) =>
    httpClient.get<AuthRole>(`/api/admin/auth/roles/${id}`),

  createRole: (data: CreateRoleRequest) =>
    httpClient.post<AuthRole>("/api/admin/auth/roles", data),

  updateRole: (id: string, data: UpdateRoleRequest) =>
    httpClient.put<AuthRole>(`/api/admin/auth/roles/${id}`, data),

  deleteRole: (id: string) =>
    httpClient.delete(`/api/admin/auth/roles/${id}`),

  // AI admin
  getAiProviders: () =>
    httpClient.get<AiProvider[]>("/api/admin/ai/providers"),

  registerAiProvider: (data: RegisterProviderRequest) =>
    httpClient.post<AiProvider>("/api/admin/ai/providers", data),

  upsertAiModel: (providerName: string, data: UpsertModelRequest) =>
    httpClient.put<AiProvider>(
      `/api/admin/ai/providers/${providerName}/models`,
      data,
    ),

  getAiRoutes: () =>
    httpClient.get<AiRuntimeRoute[]>("/api/admin/ai/routes"),

  configureAiRoute: (taskType: string, data: ConfigureRouteRequest) =>
    httpClient.put<AiRuntimeRoute>(
      `/api/admin/ai/routes/${taskType}`,
      data,
    ),
};
