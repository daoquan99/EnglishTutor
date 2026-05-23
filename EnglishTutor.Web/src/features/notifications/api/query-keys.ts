export const notificationKeys = {
  all: ["notifications"] as const,
  list: (params?: { isRead?: boolean }) =>
    [...notificationKeys.all, "list", params] as const,
  settings: () => [...notificationKeys.all, "settings"] as const,
};
