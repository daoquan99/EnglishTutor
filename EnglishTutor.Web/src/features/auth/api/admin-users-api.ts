import { httpClient } from "@/shared/api";
import type {
  AuthUserDetail,
  AuthUserListItem,
  ListUsersParams,
  PagedResponse,
  SetUserRolesRequest,
  SuspendUserRequest,
  UpdateUserRequest,
} from "../types/admin-auth";

type P = Record<string, string | number | boolean | undefined>;

export const adminUsersApi = {
  list: (params?: ListUsersParams) =>
    httpClient.get<PagedResponse<AuthUserListItem>>("/api/admin/auth/users", {
      params: params as P | undefined,
    }),

  get: (userId: string) =>
    httpClient.get<AuthUserDetail>(`/api/admin/auth/users/${userId}`),

  update: (userId: string, data: UpdateUserRequest) =>
    httpClient.put<void>(`/api/admin/auth/users/${userId}`, data),

  suspend: (userId: string, data?: SuspendUserRequest) =>
    httpClient.post<void>(`/api/admin/auth/users/${userId}/suspend`, data ?? {}),

  restore: (userId: string) =>
    httpClient.post<void>(`/api/admin/auth/users/${userId}/restore`),

  setRoles: (userId: string, data: SetUserRolesRequest) =>
    httpClient.put<void>(`/api/admin/auth/users/${userId}/roles`, data),
};
