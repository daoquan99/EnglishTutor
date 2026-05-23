export interface AuthPermission {
  id: string;
  code: string;
  description: string;
  isEnabled: boolean;
}

export interface AuthRole {
  id: string;
  name: string;
  description: string;
  isSystem: boolean;
  isEnabled: boolean;
  permissions: AuthPermission[];
}

export interface CreatePermissionRequest {
  code: string;
  description: string;
}

export interface UpdatePermissionRequest {
  description: string;
  isEnabled: boolean;
}

export interface CreateRoleRequest {
  name: string;
  description: string;
  isEnabled: boolean;
  permissionIds: string[];
}

export interface UpdateRoleRequest {
  name: string;
  description: string;
  isEnabled: boolean;
  permissionIds: string[];
}

export type UserStatusFilter = "All" | "Active" | "Suspended";

export type UserSortOrder =
  | "NewestFirst"
  | "OldestFirst"
  | "RecentlyUpdated"
  | "DisplayNameAsc";

export interface PagedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  hasMore: boolean;
}

export interface AuthUserListItem {
  id: string;
  email: string;
  displayName: string;
  isActive: boolean;
  roleIds: string[];
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface AuthUserRoleSummary {
  roleId: string;
  name: string;
}

export interface AuthUserDetail {
  id: string;
  email: string;
  displayName: string;
  isActive: boolean;
  roles: AuthUserRoleSummary[];
  effectivePermissionCodes: string[];
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface ListUsersParams {
  search?: string;
  status?: UserStatusFilter;
  roleId?: string;
  page?: number;
  pageSize?: number;
  sort?: UserSortOrder;
}

export interface UpdateUserRequest {
  displayName: string;
}

export interface SuspendUserRequest {
  reason?: string | null;
}

export interface SetUserRolesRequest {
  roleIds: string[];
}
