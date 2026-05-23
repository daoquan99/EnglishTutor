"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useAdminToast } from "@/shared/admin";
import { adminRolesApi } from "../api/admin-roles-api";
import { authAdminKeys } from "../api/query-keys";
import type {
  CreateRoleRequest,
  UpdateRoleRequest,
} from "../types/admin-auth";

export function useRoles(includeDisabled = true) {
  return useQuery({
    queryKey: authAdminKeys.roles.list(),
    queryFn: () => adminRolesApi.list(includeDisabled),
  });
}

export function useRole(id: string | null) {
  return useQuery({
    queryKey: authAdminKeys.roles.detail(id ?? ""),
    queryFn: () => adminRolesApi.get(id as string),
    enabled: !!id,
  });
}

export function useCreateRole() {
  const queryClient = useQueryClient();
  const toast = useAdminToast();
  return useMutation({
    mutationFn: (data: CreateRoleRequest) => adminRolesApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: authAdminKeys.roles.all });
      toast.success("Role đã được tạo");
    },
    onError: (err) => toast.error(err),
  });
}

export function useUpdateRole() {
  const queryClient = useQueryClient();
  const toast = useAdminToast();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateRoleRequest }) =>
      adminRolesApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: authAdminKeys.roles.all });
      toast.success("Role đã được cập nhật");
    },
    onError: (err) => toast.error(err),
  });
}

export function useDeleteRole() {
  const queryClient = useQueryClient();
  const toast = useAdminToast();
  return useMutation({
    mutationFn: (id: string) => adminRolesApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: authAdminKeys.roles.all });
      toast.success("Role đã được xoá");
    },
    onError: (err) => toast.error(err),
  });
}
