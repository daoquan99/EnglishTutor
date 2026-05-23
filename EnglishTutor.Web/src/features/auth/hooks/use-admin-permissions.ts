"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useAdminToast } from "@/shared/admin";
import { adminPermissionsApi } from "../api/admin-permissions-api";
import { authAdminKeys } from "../api/query-keys";
import type {
  CreatePermissionRequest,
  UpdatePermissionRequest,
} from "../types/admin-auth";

export function usePermissions(includeDisabled = true) {
  return useQuery({
    queryKey: authAdminKeys.permissions.list(),
    queryFn: () => adminPermissionsApi.list(includeDisabled),
  });
}

export function useCreatePermission() {
  const queryClient = useQueryClient();
  const toast = useAdminToast();
  return useMutation({
    mutationFn: (data: CreatePermissionRequest) =>
      adminPermissionsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: authAdminKeys.permissions.all,
      });
      toast.success("Permission đã được tạo");
    },
    onError: (err) => toast.error(err),
  });
}

export function useUpdatePermission() {
  const queryClient = useQueryClient();
  const toast = useAdminToast();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdatePermissionRequest }) =>
      adminPermissionsApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: authAdminKeys.permissions.all,
      });
      toast.success("Permission đã được cập nhật");
    },
    onError: (err) => toast.error(err),
  });
}

export function useDeletePermission() {
  const queryClient = useQueryClient();
  const toast = useAdminToast();
  return useMutation({
    mutationFn: (id: string) => adminPermissionsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: authAdminKeys.permissions.all,
      });
      toast.success("Permission đã được xoá");
    },
    onError: (err) => toast.error(err),
  });
}
