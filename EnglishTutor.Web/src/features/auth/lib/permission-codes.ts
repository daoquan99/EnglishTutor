/**
 * Mirror of `EnglishTutor.Modules.Auth.Contracts.Permissions.PermissionCodes`.
 * Keep in sync when adding new permission codes on the backend.
 */
export const PermissionCodes = {
  FullAccess: "admin.full_access",

  AuthUsersRead: "auth.users.read",
  AuthUsersManage: "auth.users.manage",
  AuthSessionsRead: "auth.sessions.read",
  AuthSecurityEventsRead: "auth.security-events.read",
  AuthSecurityEventsReview: "auth.security-events.review",
  AuthRolesRead: "auth.roles.read",
  AuthRolesManage: "auth.roles.manage",
  AuthPermissionsRead: "auth.permissions.read",
  AuthPermissionsManage: "auth.permissions.manage",

  AiProvidersRead: "ai.providers.read",
  AiProvidersManage: "ai.providers.manage",
  AiRoutesRead: "ai.routes.read",
  AiRoutesManage: "ai.routes.manage",
  AiPromptsRead: "ai.prompts.read",
  AiPromptsManage: "ai.prompts.manage",
  AiLogsRead: "ai.logs.read",

  ReportsRead: "reports.read",
  ReportsManage: "reports.manage",

  MistakesRead: "mistakes.read",
  AssessmentsRead: "assessments.read",
} as const;

export type PermissionCode = (typeof PermissionCodes)[keyof typeof PermissionCodes];

/**
 * Permissions that grant access to the `(admin)` area in any form.
 * Anyone with at least one of these can enter the admin layout.
 */
export const ADMIN_AREA_PERMISSIONS: readonly PermissionCode[] = [
  PermissionCodes.FullAccess,
  PermissionCodes.AuthUsersRead,
  PermissionCodes.AuthUsersManage,
  PermissionCodes.AuthRolesRead,
  PermissionCodes.AuthRolesManage,
  PermissionCodes.AuthPermissionsRead,
  PermissionCodes.AuthPermissionsManage,
  PermissionCodes.AuthSessionsRead,
  PermissionCodes.AuthSecurityEventsRead,
  PermissionCodes.AuthSecurityEventsReview,
  PermissionCodes.AiProvidersRead,
  PermissionCodes.AiProvidersManage,
  PermissionCodes.AiRoutesRead,
  PermissionCodes.AiRoutesManage,
  PermissionCodes.AiLogsRead,
  PermissionCodes.ReportsRead,
  PermissionCodes.ReportsManage,
  PermissionCodes.MistakesRead,
  PermissionCodes.AssessmentsRead,
];
