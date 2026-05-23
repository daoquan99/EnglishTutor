export { useCurrentUser } from "./hooks/use-current-user";
export { useLogin } from "./hooks/use-login";
export { useRegister } from "./hooks/use-register";
export { useLogout } from "./hooks/use-logout";
export { useAuthStore } from "./hooks/use-auth-store";
export { LoginForm } from "./components/login-form";
export { RegisterForm } from "./components/register-form";
export { UserMenu } from "./components/user-menu";
export type { CurrentUser, LoginRequest, RegisterRequest } from "./types/auth";

// Admin surface
export {
  useRoles,
  useRole,
  useCreateRole,
  useUpdateRole,
  useDeleteRole,
} from "./hooks/use-admin-roles";
export {
  usePermissions,
  useCreatePermission,
  useUpdatePermission,
  useDeletePermission,
} from "./hooks/use-admin-permissions";
export {
  useAdminUsers,
  useAdminUserDetail,
  useUpdateUser,
  useSuspendUser,
  useRestoreUser,
  useSetUserRoles,
} from "./hooks/use-admin-users";

export { RolesTable } from "./components/admin/roles-table";
export { PermissionsTable } from "./components/admin/permissions-table";
export { UsersTable } from "./components/admin/users-table";
export {
  EditUserDialog,
  SuspendUserDialog,
  RestoreUserDialog,
  SetRolesDialog,
} from "./components/admin/user-action-dialogs";
export { UserDetailSheet } from "./components/admin/user-detail-sheet";

export type {
  AuthPermission,
  AuthRole,
  AuthUserDetail,
  AuthUserListItem,
  AuthUserRoleSummary,
  CreatePermissionRequest,
  CreateRoleRequest,
  ListUsersParams,
  PagedResponse,
  SetUserRolesRequest,
  SuspendUserRequest,
  UpdatePermissionRequest,
  UpdateRoleRequest,
  UpdateUserRequest,
  UserSortOrder,
  UserStatusFilter,
} from "./types/admin-auth";

export {
  PermissionCodes,
  ADMIN_AREA_PERMISSIONS,
  type PermissionCode,
} from "./lib/permission-codes";
