export const adminKeys = {
  all: ["admin"] as const,
  users: (params?: Record<string, unknown>) =>
    [...adminKeys.all, "users", params] as const,
  aiUsage: (params?: Record<string, unknown>) =>
    [...adminKeys.all, "ai-usage", params] as const,
  learningActivity: (params?: Record<string, unknown>) =>
    [...adminKeys.all, "learning-activity", params] as const,
  commonMistakes: (params?: Record<string, unknown>) =>
    [...adminKeys.all, "mistakes", params] as const,
  assessmentPassRates: (params?: Record<string, unknown>) =>
    [...adminKeys.all, "assessments", params] as const,
  auditLogs: (params?: Record<string, unknown>) =>
    [...adminKeys.all, "audit-logs", params] as const,
  deadLetters: (params?: Record<string, unknown>) =>
    [...adminKeys.all, "dead-letters", params] as const,
  permissions: () => [...adminKeys.all, "permissions"] as const,
  roles: () => [...adminKeys.all, "roles"] as const,
  aiProviders: () => [...adminKeys.all, "ai-providers"] as const,
  aiRoutes: () => [...adminKeys.all, "ai-routes"] as const,
};
