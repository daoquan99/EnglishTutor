"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useAdminToast } from "@/shared/admin";
import { adminUsersApi } from "../api/admin-users-api";
import { authAdminKeys } from "../api/query-keys";
import type {
  ListUsersParams,
  SetUserRolesRequest,
  SuspendUserRequest,
  UpdateUserRequest,
} from "../types/admin-auth";

export function useAdminUsers(params?: ListUsersParams) {
  return useQuery({
    queryKey: authAdminKeys.users.list(params as Record<string, unknown>),
    queryFn: () => adminUsersApi.list(params),
    placeholderData: (previousData) => previousData,
  });
}

export function useAdminUserDetail(userId: string | null) {
  return useQuery({
    queryKey: authAdminKeys.users.detail(userId ?? ""),
    queryFn: () => adminUsersApi.get(userId as string),
    enabled: !!userId,
  });
}

function invalidateUsers(queryClient: ReturnType<typeof useQueryClient>) {
  queryClient.invalidateQueries({ queryKey: authAdminKeys.users.all });
}

export function useUpdateUser(userId: string) {
  const queryClient = useQueryClient();
  const toast = useAdminToast();
  return useMutation({
    mutationFn: (data: UpdateUserRequest) => adminUsersApi.update(userId, data),
    onSuccess: () => {
      invalidateUsers(queryClient);
      queryClient.invalidateQueries({
        queryKey: authAdminKeys.users.detail(userId),
      });
      toast.success("User đã được cập nhật");
    },
    onError: (err) => toast.error(err),
  });
}

export function useSuspendUser(userId: string) {
  const queryClient = useQueryClient();
  const toast = useAdminToast();
  return useMutation({
    mutationFn: (data?: SuspendUserRequest) =>
      adminUsersApi.suspend(userId, data),
    onSuccess: () => {
      invalidateUsers(queryClient);
      queryClient.invalidateQueries({
        queryKey: authAdminKeys.users.detail(userId),
      });
      toast.success("User đã bị suspend");
    },
    onError: (err) => toast.error(err),
  });
}

export function useRestoreUser() {
  const queryClient = useQueryClient();
  const toast = useAdminToast();
  return useMutation({
    mutationFn: (userId: string) => adminUsersApi.restore(userId),
    onSuccess: (_data, userId) => {
      invalidateUsers(queryClient);
      queryClient.invalidateQueries({
        queryKey: authAdminKeys.users.detail(userId),
      });
      toast.success("User đã được khôi phục");
    },
    onError: (err) => toast.error(err),
  });
}

export function useSetUserRoles(userId: string) {
  const queryClient = useQueryClient();
  const toast = useAdminToast();
  return useMutation({
    mutationFn: (data: SetUserRolesRequest) =>
      adminUsersApi.setRoles(userId, data),
    onSuccess: () => {
      invalidateUsers(queryClient);
      queryClient.invalidateQueries({
        queryKey: authAdminKeys.users.detail(userId),
      });
      toast.success("Đã cập nhật role cho user");
    },
    onError: (err) => toast.error(err),
  });
}
