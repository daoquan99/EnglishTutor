import { httpClient } from "@/shared/api";
import type {
  AuthPermission,
  CreatePermissionRequest,
  UpdatePermissionRequest,
} from "../types/admin-auth";

export const adminPermissionsApi = {
  list: (includeDisabled = false) =>
    httpClient.get<AuthPermission[]>("/api/admin/auth/permissions", {
      params: { includeDisabled },
    }),

  create: (data: CreatePermissionRequest) =>
    httpClient.post<AuthPermission>("/api/admin/auth/permissions", data),

  update: (id: string, data: UpdatePermissionRequest) =>
    httpClient.put<AuthPermission>(`/api/admin/auth/permissions/${id}`, data),

  delete: (id: string) =>
    httpClient.delete(`/api/admin/auth/permissions/${id}`),
};
