import { httpClient } from "@/shared/api";
import type {
  AuthRole,
  CreateRoleRequest,
  UpdateRoleRequest,
} from "../types/admin-auth";

export const adminRolesApi = {
  list: (includeDisabled = false) =>
    httpClient.get<AuthRole[]>("/api/admin/auth/roles", {
      params: { includeDisabled },
    }),

  get: (id: string) => httpClient.get<AuthRole>(`/api/admin/auth/roles/${id}`),

  create: (data: CreateRoleRequest) =>
    httpClient.post<AuthRole>("/api/admin/auth/roles", data),

  update: (id: string, data: UpdateRoleRequest) =>
    httpClient.put<AuthRole>(`/api/admin/auth/roles/${id}`, data),

  delete: (id: string) => httpClient.delete(`/api/admin/auth/roles/${id}`),
};
