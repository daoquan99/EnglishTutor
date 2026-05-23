export {
  AdminPageHeader,
  type AdminBreadcrumbItem,
} from "./components/admin-page-header";
export {
  AdminDataTable,
  type AdminDataTableColumn,
  type AdminDataTablePagination,
  type AdminDataTableSearch,
} from "./components/admin-data-table";
export { AdminCrudDialog } from "./components/admin-crud-dialog";
export { AdminDeleteConfirmDialog } from "./components/admin-delete-confirm-dialog";
export { AdminPermissionGate } from "./components/admin-permission-gate";
export { AdminAccessDenied } from "./components/admin-access-denied";
export { AdminEmptyState } from "./components/admin-empty-state";
export {
  useAdminPermission,
  ADMIN_WILDCARD_PERMISSION,
  type UseAdminPermissionResult,
} from "./hooks/use-admin-permission";
export {
  useAdminToast,
  extractApiErrorMessage,
  type UseAdminToastResult,
} from "./hooks/use-admin-toast";
