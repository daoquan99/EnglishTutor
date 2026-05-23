import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { adminApi } from "../api/admin-api";
import { adminKeys } from "../api/query-keys";
import type {
  CreatePermissionRequest,
  UpdatePermissionRequest,
  CreateRoleRequest,
  UpdateRoleRequest,
} from "../types/admin-reports";

export function usePermissions() {
  return useQuery({
    queryKey: adminKeys.permissions(),
    queryFn: () => adminApi.getPermissions(true),
  });
}

export function useCreatePermission() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreatePermissionRequest) =>
      adminApi.createPermission(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.permissions() });
      toast.success("Permission created");
    },
  });
}

export function useUpdatePermission() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdatePermissionRequest }) =>
      adminApi.updatePermission(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.permissions() });
      toast.success("Permission updated");
    },
  });
}

export function useDeletePermission() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => adminApi.deletePermission(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.permissions() });
      toast.success("Permission deleted");
    },
  });
}

export function useRoles() {
  return useQuery({
    queryKey: adminKeys.roles(),
    queryFn: () => adminApi.getRoles(true),
  });
}

export function useCreateRole() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateRoleRequest) => adminApi.createRole(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.roles() });
      toast.success("Role created");
    },
  });
}

export function useUpdateRole() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateRoleRequest }) =>
      adminApi.updateRole(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.roles() });
      toast.success("Role updated");
    },
  });
}

export function useDeleteRole() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => adminApi.deleteRole(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.roles() });
      toast.success("Role deleted");
    },
  });
}
